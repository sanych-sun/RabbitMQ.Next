using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
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

        private ISocket socket;

        private int heartbeatInterval;
        private DateTimeOffset heartbeatTimeout = DateTimeOffset.MaxValue;

        private static readonly byte[] FrameEndPayload = { ProtocolConstants.FrameEndByte };

        public Connection(ConnectionString connectionString)
        {
            this.State = ConnectionState.Pending;
            this.BufferPool = new BufferPool(ProtocolConstants.FrameMinSize);
            this.stateChanged = new EventSource<ConnectionStateChanged>();
            var registryBuilder = new MethodRegistryBuilder();
            registryBuilder.AddConnectionMethods();
            registryBuilder.AddChannelMethods();
            registryBuilder.AddExchangeMethods();
            registryBuilder.AddQueueMethods();

            var methodRegistry = registryBuilder.Build();

            Func<int, Channel> channelFactory = (channelNumber) =>
            {
                var pipe = new Pipe();
                var methodSender = new FrameSender(this, methodRegistry, (ushort) channelNumber, this.BufferPool);
                return new Channel(pipe, methodRegistry, methodSender);
            };

            this.channelPool = new ChannelPool(channelFactory);
            this.writerSemaphore = new SemaphoreSlim(1, 1);
            this.connectionString = connectionString;
        }

        public async Task ConnectAsync()
        {
            await this.ChangeStateAsync(ConnectionState.Connecting);
            this.socket = await EndpointResolver.OpenSocketAsync(this.connectionString.EndPoints);

            await this.ChangeStateAsync(ConnectionState.Negotiating);
            var connectionChannel = this.channelPool.Next();

            var socketReadThread = new Thread(this.ReceiveLoop);
            socketReadThread.Name = "RabbitMQ socket reader";
            socketReadThread.Start();


            await this.SendProtocolHeaderAsync();

            var negotiationResults = await connectionChannel.UseSyncChannel(async (ch, connection) =>
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
                await ch.SendAsync<OpenMethod>(new OpenMethod(connection.VirtualHost));
                await openOkTask;

                return new ConnectionNegotiationResults(tuneMethod.ChannelMax, tuneMethod.MaxFrameSize, tuneMethod.HeartbeatInterval);
            }, this.connectionString);

            this.heartbeatInterval = negotiationResults.HeartbeatInterval / 2;
            this.BufferPool.SetChunkSize((int)negotiationResults.MaxFrameSize);

            this.ScheduleHeartBeat();
            await this.ChangeStateAsync(ConnectionState.Configuring);
            await this.ChangeStateAsync(ConnectionState.Open);
        }

        public ConnectionState State { get; private set; }

        public BufferPool BufferPool { get; }

        public IEventSource<ConnectionStateChanged> StateChanged => this.stateChanged;

        public async Task<IChannel> CreateChannelAsync()
        {
            // TODO: validate state

            var channel = this.channelPool.Next();
            await channel.SendAsync<Methods.Channel.OpenMethod, Methods.Channel.OpenOkMethod>(new Methods.Channel.OpenMethod());

            return channel;
        }

        public async Task CloseAsync()
        {
            // todo: validate state here

            await this.channelPool[0].SendAsync<CloseMethod, CloseOkMethod>(new CloseMethod((ushort)ReplyCode.Success, "Goodbye", 0));

            this.CleanUpOnSocketClose();

            await this.ChangeStateAsync(ConnectionState.Closed);
        }


        public async Task SendAsync<TState>(TState state, Func<Func<ReadOnlyMemory<byte>, ValueTask>, TState, ValueTask> writer, CancellationToken cancellation = default)
        {
            await this.writerSemaphore.WaitAsync(cancellation);
            try
            {
                await writer(this.socket.SendAsync, state);
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
                : DateTimeOffset.UtcNow.AddSeconds(this.heartbeatInterval);
        }

        private async Task SendProtocolHeaderAsync()
        {
            await this.socket.SendAsync(ProtocolConstants.AmqpHeader);
        }

        private void ReceiveLoop()
        {
            var headerBuffer = new byte[ProtocolConstants.FrameHeaderSize];
            var endFrameBuffer = new byte[1];

            while (true)
            {
                // 0. Check if need to sent heartbeat
                if (this.heartbeatTimeout < DateTimeOffset.UtcNow)
                {
                    Task.Run(() => this.SendAsync(ProtocolConstants.HeartbeatFrame, default));
                }

                // 1. Read frame header
                if (this.socket.FillBuffer(headerBuffer) != SocketError.Success)
                {
                    this.CleanUpOnSocketClose();
                    return;
                }

                var header = ((ReadOnlySpan<byte>)headerBuffer).ReadFrameHeader();

                // 2. Choose appropriate channel to forward the data
                var targetPipe = this.channelPool[header.Channel].Pipe.Writer;
                targetPipe.Write(headerBuffer);

                // 3. Read frame payload into the channel
                if (this.socket.Receive(targetPipe, header.PayloadSize) != SocketError.Success)
                {
                    this.CleanUpOnSocketClose();
                    return;
                }

                // 4. Ensure there is FrameEnd
                if (this.socket.FillBuffer(endFrameBuffer) != SocketError.Success)
                {
                    this.CleanUpOnSocketClose();
                    return;
                }
                if (endFrameBuffer[0] != ProtocolConstants.FrameEndByte)
                {
                    // TODO: throw connection exception here
                    throw new InvalidOperationException();
                }

                // 5. Make data available for consumers
                targetPipe.FlushAsync().GetAwaiter().GetResult();
            }
        }

        private void CleanUpOnSocketClose()
        {
            this.socket.Dispose();
            this.socket = null;
            this.channelPool.ReleaseAll();
        }
    }
}