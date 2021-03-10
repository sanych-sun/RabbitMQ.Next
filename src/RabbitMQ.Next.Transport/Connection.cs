using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Buffers;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Transport.Buffers;
using RabbitMQ.Next.Transport.Channels;
using RabbitMQ.Next.Transport.Events;
using RabbitMQ.Next.Transport.Methods.Connection;
using RabbitMQ.Next.Transport.Methods.Registry;
using RabbitMQ.Next.Transport.Sockets;
using RabbitMQ.Next.Transport.Methods;

namespace RabbitMQ.Next.Transport
{
    internal class Connection : IConnection, ISocketWriter
    {
        private readonly SemaphoreSlim writerSemaphore;
        private readonly ChannelPool channelPool;
        private readonly ConnectionString connectionString;
        private readonly EventSource<ConnectionStateChanged> stateChanged;
        private readonly IBufferPoolInternal bufferPool;
        private readonly ConnectionDetails connectionDetails = new ConnectionDetails();

        private ISocket socket;
        private CancellationTokenSource socketIoCancellation;

        private int heartbeatIntervalMs;
        private DateTimeOffset heartbeatTimeout = DateTimeOffset.MaxValue;

        private static readonly byte[] FrameEndPayload = { ProtocolConstants.FrameEndByte };

        public Connection(ConnectionString connectionString)
        {
            this.State = ConnectionState.Pending;
            this.bufferPool = new BufferPool(ProtocolConstants.FrameMinSize);
            this.stateChanged = new EventSource<ConnectionStateChanged>();
            var registryBuilder = new MethodRegistryBuilder();
            registryBuilder.AddConnectionMethods();
            registryBuilder.AddChannelMethods();
            registryBuilder.AddExchangeMethods();
            registryBuilder.AddQueueMethods();
            registryBuilder.AddBasicMethods();

            this.MethodRegistry = registryBuilder.Build();

            Func<int, IEnumerable<IFrameHandler>, Channel> channelFactory = (channelNumber, handlers) =>
            {
                var pipe = new Pipe();
                var methodSender = new FrameSender(this, this, this.MethodRegistry, (ushort) channelNumber, this.bufferPool);
                return new Channel(pipe, this.MethodRegistry, methodSender, handlers);
            };

            this.channelPool = new ChannelPool(channelFactory);
            this.writerSemaphore = new SemaphoreSlim(1, 1);
            this.connectionString = connectionString;
        }

        public async Task ConnectAsync()
        {
            this.socketIoCancellation = new CancellationTokenSource();
            // TODO: adopt authentication_failure_close capability to handle auth errors

            await this.ChangeStateAsync(ConnectionState.Connecting);
            this.socket = await EndpointResolver.OpenSocketAsync(this.connectionString.EndPoints, this.connectionString.Ssl);

            await this.ChangeStateAsync(ConnectionState.Negotiating);
            var connectionChannel = this.channelPool.Next();

            Task.Run(() => this.ReceiveLoop(socketIoCancellation.Token));

            await this.SendProtocolHeaderAsync();

            var negotiationResults = await connectionChannel.UseSyncChannel(this.connectionString, async (ch, connection) =>
            {
                var startMethod = ch.WaitAsync<StartMethod>();
                // TODO: make it dynamic based on assembly version and allow add extra properties
                var clientProperties = new Dictionary<string, object>()
                {
                    ["product"] = "RabbitMQ.Next.Transport",
                    ["version"] = "0.1.0",
                    ["platform"] = Environment.OSVersion.ToString(),
                };

                var tuneMethodTask = ch.WaitAsync<TuneMethod>();
                await ch.SendAsync(new StartOkMethod("PLAIN", $"\0{connection.UserName}\0{connection.Password}", "en-US", clientProperties));

                var tuneMethod = await tuneMethodTask;

                await ch.SendAsync(new TuneOkMethod(tuneMethod.ChannelMax, tuneMethod.MaxFrameSize, tuneMethod.HeartbeatInterval));
                var openOkTask = ch.WaitAsync<OpenOkMethod>();
                await ch.SendAsync(new OpenMethod(connection.VirtualHost));
                await openOkTask;

                return new ConnectionNegotiationResults(tuneMethod.ChannelMax, tuneMethod.MaxFrameSize, tuneMethod.HeartbeatInterval);
            });

            this.heartbeatIntervalMs = negotiationResults.HeartbeatInterval * 1000 / 2;
            this.bufferPool.SetBufferSize((int)negotiationResults.MaxFrameSize);

            this.connectionDetails.HeartbeatInterval = negotiationResults.HeartbeatInterval;
            this.connectionDetails.FrameMaxSize = (int) negotiationResults.MaxFrameSize;

            // start heartbeat
            Task.Run(() => this.HeartbeatLoop(socketIoCancellation.Token));

            this.ScheduleHeartBeat();
            await this.ChangeStateAsync(ConnectionState.Configuring);
            await this.ChangeStateAsync(ConnectionState.Open);
        }

        public ConnectionState State { get; private set; }

        public IMethodRegistry MethodRegistry { get; }

        public IBufferPool BufferPool => this.bufferPool;
        public IConnectionDetails Details => this.connectionDetails;

        public IEventSource<ConnectionStateChanged> StateChanged => this.stateChanged;

        public async Task<IChannel> CreateChannelAsync(IEnumerable<IFrameHandler> handlers = null, CancellationToken cancellationToken = default)
        {
            // TODO: validate state

            var channel = this.channelPool.Next(handlers);
            await channel.SendAsync<Methods.Channel.OpenMethod, Methods.Channel.OpenOkMethod>(new Methods.Channel.OpenMethod(), cancellationToken);

            return channel;
        }

        public async Task CloseAsync()
        {
            // todo: validate state here

            await this.channelPool[0].SendAsync<CloseMethod, CloseOkMethod>(new CloseMethod((ushort)ReplyCode.Success, "Goodbye", 0));

            this.CleanUpOnSocketClose();

            await this.ChangeStateAsync(ConnectionState.Closed);
        }

        public ValueTask DisposeAsync() => new ValueTask(this.CloseAsync());


        public async Task SendAsync<TState>(TState state, Func<Func<ReadOnlyMemory<byte>, ValueTask>, TState, ValueTask> writer, CancellationToken cancellation = default)
        {
            await this.writerSemaphore.WaitAsync(cancellation);
            try
            {
                await writer.Invoke(this.socket.SendAsync, state);
                await this.socket.SendAsync(FrameEndPayload);
                this.ScheduleHeartBeat();
            }
            finally
            {
                this.writerSemaphore.Release();
            }
        }

        private async ValueTask ChangeStateAsync(ConnectionState newState)
        {
            if (this.State == newState)
            {
                return;
            }

            this.State = newState;
            await this.stateChanged.InvokeAsync(new ConnectionStateChanged(newState));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ScheduleHeartBeat()
        {
            this.heartbeatTimeout = (this.State != ConnectionState.Open)
                ? DateTimeOffset.MaxValue
                : DateTimeOffset.UtcNow.AddMilliseconds(this.heartbeatIntervalMs);
        }

        private async Task SendProtocolHeaderAsync()
        {
            await this.socket.SendAsync(ProtocolConstants.AmqpHeader);
        }

        private async Task HeartbeatLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (this.heartbeatTimeout < DateTimeOffset.UtcNow)
                {
                    await this.SendAsync(ProtocolConstants.HeartbeatFrame);
                }

                await Task.Delay(this.heartbeatIntervalMs, cancellationToken);
            }
        }

        private async Task ReceiveLoop(CancellationToken cancellationToken)
        {
            var headerBuffer = new byte[ProtocolConstants.FrameHeaderSize];
            var endFrameBuffer = new byte[1];

            const int customHeaderSize = 12;
            Memory<byte> contentHeaderCustomHeader = new byte[customHeaderSize];

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // 1. Read frame header
                    await this.socket.FillBufferAsync(headerBuffer, cancellationToken);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    ((ReadOnlySpan<byte>) headerBuffer).ReadFrameHeader(out FrameType frameType, out ushort channel, out uint payloadSize);

                    // 2. Choose appropriate channel to forward the data
                    var targetPipe = this.channelPool[channel].Pipe;

                    // 3. Read frame payload into the channel
                    switch (frameType)
                    {
                        case FrameType.Method:
                            targetPipe.BeginLogicalFrame(FrameType.Method, (int) payloadSize);
                            await this.socket.FillBufferAsync(targetPipe, (int)payloadSize, cancellationToken);
                            break;
                        case FrameType.ContentHeader:
                            await this.socket.FillBufferAsync(contentHeaderCustomHeader);

                            ((ReadOnlySpan<byte>)(contentHeaderCustomHeader
                                .Slice(4) // skip 2 obsolete shorts
                                .Span)).Read(out ulong contentSide);

                            var headerSize = payloadSize - customHeaderSize;
                            targetPipe.BeginLogicalFrame(FrameType.ContentHeader, (int)headerSize);
                            await this.socket.FillBufferAsync(targetPipe, (int)headerSize);

                            targetPipe.BeginLogicalFrame(FrameType.ContentBody, (int)contentSide);
                            break;
                        case FrameType.ContentBody:
                            await this.socket.FillBufferAsync(targetPipe, (int) payloadSize, cancellationToken);
                            break;
                    }

                    // 4. Ensure there is FrameEnd
                    await this.socket.FillBufferAsync(endFrameBuffer);

                    if (endFrameBuffer[0] != ProtocolConstants.FrameEndByte)
                    {
                        // TODO: throw connection exception here
                        throw new InvalidOperationException();
                    }
                }
            }
            catch (Exception)
            {
                this.CleanUpOnSocketClose();
                // todo: report to diagnostic source
            }
        }

        private void CleanUpOnSocketClose()
        {
            if (this.socketIoCancellation == null || this.socketIoCancellation.IsCancellationRequested)
            {
                return;
            }

            this.socketIoCancellation.Cancel();
            this.socket.Dispose();
            this.socket = null;
        }
    }
}