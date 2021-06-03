using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Buffers;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Events;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Buffers;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Events;
using RabbitMQ.Next.Sockets;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods.Connection;

namespace RabbitMQ.Next
{
    internal class Connection : IConnection
    {
        private readonly IReadOnlyList<Endpoint> endpoints;
        private readonly string virtualHost;
        private readonly IAuthMechanism authMechanism;
        private readonly IReadOnlyDictionary<string, object> clientProperties;
        private readonly string locale;
        private readonly IMethodRegistry methodRegistry;

        private readonly ChannelPool channelPool;
        private readonly EventSource<ConnectionState> stateChanged;
        private readonly BufferManager bufferManager;
        private readonly IBufferPool bufferPool;
        private readonly ConnectionDetails connectionDetails = new ConnectionDetails();

        private ISocket socket;
        private IFrameSender frameSender;
        private CancellationTokenSource socketIoCancellation;

        public Connection(
            IReadOnlyList<Endpoint> endpoints,
            string virtualHost,
            IAuthMechanism authMechanism,
            string locale,
            IReadOnlyDictionary<string, object> clientProperties,
            IMethodRegistry methodRegistry)
        {
            this.endpoints = endpoints;
            this.virtualHost = virtualHost;
            this.authMechanism = authMechanism;
            this.locale = locale;
            this.clientProperties = clientProperties;
            this.methodRegistry = methodRegistry;

            this.State = ConnectionState.Pending;
            this.bufferManager = new BufferManager(ProtocolConstants.FrameMinSize);
            this.bufferPool = new BufferPool(this.bufferManager);
            this.stateChanged = new EventSource<ConnectionState>();
            this.channelPool = new ChannelPool();
        }

        public async Task ConnectAsync()
        {
            // TODO: adopt authentication_failure_close capability to handle auth errors

            await this.ChangeStateAsync(ConnectionState.Connecting);
            this.socket = await EndpointResolver.OpenSocketAsync(this.endpoints);
            this.frameSender = new FrameSender(this.socket, this.methodRegistry, this.BufferPool);

            await this.ChangeStateAsync(ConnectionState.Negotiating);
            var connectionChannel = new Channel(this.channelPool, this.methodRegistry, this.frameSender, this.bufferPool, null);

            this.socketIoCancellation = new CancellationTokenSource();
            Task.Run(() => this.ReceiveLoop(this.socketIoCancellation.Token));

            var negotiationResults = await connectionChannel.UseChannel(async ch =>
            {
                var startMethodTask = ch.WaitAsync<StartMethod>();
                await this.SendProtocolHeaderAsync();
                var startMethod = await startMethodTask;

                var tuneMethodTask = ch.WaitAsync<TuneMethod>();

                if (!startMethod.Mechanisms.Contains(this.authMechanism.Type))
                {
                    throw new NotSupportedException("Provided auth mechanism does not supported by the server");
                }

                await ch.SendAsync(new StartOkMethod(this.authMechanism.Type, this.authMechanism.ToResponse(), this.locale, this.clientProperties));

                var tuneMethod = await tuneMethodTask;

                await ch.SendAsync(new TuneOkMethod(tuneMethod.ChannelMax, tuneMethod.MaxFrameSize, tuneMethod.HeartbeatInterval));

                // todo: handle wrong vhost name
                await ch.SendAsync<OpenMethod, OpenOkMethod>(new OpenMethod(this.virtualHost));

                return (tuneMethod.ChannelMax, tuneMethod.MaxFrameSize, tuneMethod.HeartbeatInterval);
            });

            var heartbeatIntervalMs = negotiationResults.HeartbeatInterval * 1000;
            this.bufferManager.SetBufferSize((int)negotiationResults.MaxFrameSize);

            this.connectionDetails.HeartbeatInterval = negotiationResults.HeartbeatInterval;
            this.connectionDetails.FrameMaxSize = (int) negotiationResults.MaxFrameSize;

            // start heartbeat
            Task.Run(() => this.HeartbeatLoop(heartbeatIntervalMs, this.socketIoCancellation.Token));

            await this.ChangeStateAsync(ConnectionState.Configuring);
            await this.ChangeStateAsync(ConnectionState.Open);
        }

        public ConnectionState State { get; private set; }

        public IBufferPool BufferPool => this.bufferPool;

        public IConnectionDetails Details => this.connectionDetails;

        public IEventSource<ConnectionState> StateChanged => this.stateChanged;

        public async Task<IChannel> CreateChannelAsync(IReadOnlyList<IMethodHandler> handlers = null, CancellationToken cancellationToken = default)
        {
            // TODO: validate state

            var channel = new Channel(this.channelPool, this.methodRegistry, this.frameSender, this.bufferPool, handlers);
            await channel.SendAsync<Transport.Methods.Channel.OpenMethod, Transport.Methods.Channel.OpenOkMethod>(new Transport.Methods.Channel.OpenMethod(), cancellationToken);

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

        private async ValueTask ChangeStateAsync(ConnectionState newState)
        {
            if (this.State == newState)
            {
                return;
            }

            this.State = newState;
            await this.stateChanged.InvokeAsync(newState);
        }

        private async Task SendProtocolHeaderAsync()
        {
            await this.socket.SendAsync(ProtocolConstants.AmqpHeader);
        }

        private async Task HeartbeatLoop(int heartbeatIntervalMs, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(heartbeatIntervalMs, cancellationToken);
                await this.frameSender.SendHeartBeatAsync();
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
                    var targetWriter = this.channelPool[channel].Writer;

                    // 3. Read frame payload into the channel
                    switch (frameType)
                    {
                        case FrameType.Method:
                            ChannelFrame.WriteHeader(targetWriter, ChannelFrameType.Method, payloadSize);
                            await this.socket.FillBufferAsync(targetWriter, (int) payloadSize, cancellationToken);
                            await targetWriter.FlushAsync();
                            break;
                        case FrameType.ContentHeader:
                            await this.socket.FillBufferAsync(contentHeaderCustomHeader);

                            ((ReadOnlySpan<byte>) (contentHeaderCustomHeader
                                .Slice(4) // skip 2 obsolete shorts
                                .Span)).Read(out ulong contentSide);

                            var headerSize = payloadSize - customHeaderSize;
                            ChannelFrame.WriteHeader(targetWriter, ChannelFrameType.Content, sizeof(uint) + headerSize + (uint) contentSide);
                            targetWriter.GetMemory(sizeof(int)).Span.Write((int) headerSize);
                            targetWriter.Advance(sizeof(int));
                            await this.socket.FillBufferAsync(targetWriter, (int) headerSize);
                            break;
                        case FrameType.ContentBody:
                            await this.socket.FillBufferAsync(targetWriter, (int) payloadSize, cancellationToken);
                            await targetWriter.FlushAsync();
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
            catch (SocketException)
            {
                // todo: report to diagnostic source

                this.CleanUpOnSocketClose();
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

            this.channelPool.ReleaseAll();
        }
    }
}