using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Buffers;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Exceptions;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Buffers;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Sockets;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods.Connection;

namespace RabbitMQ.Next
{
    internal class Connection : IConnection
    {
        private readonly IMethodRegistry methodRegistry;

        private readonly ChannelPool channelPool;
        private readonly BufferManager bufferManager;
        private readonly IBufferPool bufferPool;
        private readonly ConnectionDetails connectionDetails = new ConnectionDetails();

        private IFrameSender frameSender;
        private CancellationTokenSource socketIoCancellation;

        private Connection(IMethodRegistry methodRegistry)
        {
            this.methodRegistry = methodRegistry;

            this.State = ConnectionState.Pending;
            this.bufferManager = new BufferManager(ProtocolConstants.FrameMinSize);
            this.bufferPool = new BufferPool(this.bufferManager);
            this.channelPool = new ChannelPool();
        }

        public static async Task<IConnection> ConnectAsync(IReadOnlyList<Endpoint> endpoints,
            string virtualHost,
            IAuthMechanism authMechanism,
            string locale,
            IReadOnlyDictionary<string, object> clientProperties,
            IMethodRegistry methodRegistry)
        {
            var connection = new Connection(methodRegistry);
            // TODO: adopt authentication_failure_close capability to handle auth errors

            connection.State = ConnectionState.Connecting;
            var socket = await OpenSocketAsync(endpoints);
            connection.frameSender = new FrameSender(socket, connection.methodRegistry, connection.BufferPool);

            connection.State = ConnectionState.Negotiating;
            var connectionChannel = new Channel(connection.channelPool, connection.methodRegistry, connection.frameSender, connection.bufferPool, null);

            connection.socketIoCancellation = new CancellationTokenSource();
            Task.Run(() => connection.ReceiveLoop(socket, connection.socketIoCancellation.Token));

            var negotiationResults = await connectionChannel.UseChannel(async ch =>
            {
                // connection should be forcibly closed if negotiation phase take more then 10s.
                var cancellation = new CancellationTokenSource(10000);
                var negotiateTask = NegotiateAsync(ch, virtualHost, locale, authMechanism, clientProperties, cancellation.Token);
                await connection.frameSender.SendAmqpHeaderAsync();

                return await negotiateTask;
            });

            var heartbeatIntervalMs = negotiationResults.HeartbeatInterval * 1000;
            connection.bufferManager.SetBufferSize((int)negotiationResults.MaxFrameSize);
            connection.frameSender.FrameMaxSize = (int) negotiationResults.MaxFrameSize;

            connection.connectionDetails.HeartbeatInterval = negotiationResults.HeartbeatInterval;
            connection.connectionDetails.FrameMaxSize = (int) negotiationResults.MaxFrameSize;

            connectionChannel.OnCompleted(connection.ConnectionClose);

            // start heartbeat
            Task.Run(() => connection.HeartbeatLoop(heartbeatIntervalMs, connection.socketIoCancellation.Token));

            connection.State = ConnectionState.Configuring;
            connection.State = ConnectionState.Open;

            return connection;
        }

        public ConnectionState State { get; private set; }

        public IBufferPool BufferPool => this.bufferPool;

        public IConnectionDetails Details => this.connectionDetails;

        public async Task<IChannel> OpenChannelAsync(IReadOnlyList<IMethodHandler> handlers = null, CancellationToken cancellationToken = default)
        {
            // TODO: validate state

            var channel = new Channel(this.channelPool, this.methodRegistry, this.frameSender, this.bufferPool, handlers);
            await channel.SendAsync<Transport.Methods.Channel.OpenMethod, Transport.Methods.Channel.OpenOkMethod>(new Transport.Methods.Channel.OpenMethod(), cancellationToken);

            return channel;
        }

        public async ValueTask DisposeAsync()
        {
            // todo: validate state here

            if (this.State != ConnectionState.Open)
            {
                return;
            }

            await this.channelPool[0].SendAsync<CloseMethod, CloseOkMethod>(new CloseMethod((ushort)ReplyCode.Success, "Goodbye", 0));

            this.ConnectionClose(null);
        }

        private static async Task<ISocket> OpenSocketAsync(IReadOnlyList<Endpoint> endpoints)
        {
            List<Exception> exceptions = null;

            foreach (var endpoint in endpoints)
            {
                try
                {
                    return await EndpointResolver.OpenSocketAsync(endpoint);
                }
                catch (Exception e)
                {
                    // todo: report exception to diagnostic source
                    exceptions ??= new List<Exception>();
                    exceptions.Add(new EndPointResolutionException(endpoint.ToUri(), e));
                }
            }

            throw new AggregateException("Cannot establish connection RabbitMQ cluster. See inner exceptions for more details.", exceptions);
        }

        private async Task HeartbeatLoop(int heartbeatIntervalMs, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(heartbeatIntervalMs, cancellationToken);
                await this.frameSender.SendHeartBeatAsync();
            }
        }

        private async Task ReceiveLoop(ISocket socket, CancellationToken cancellationToken)
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
                    await socket.FillBufferAsync(headerBuffer, cancellationToken);
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
                            await socket.FillBufferAsync(targetWriter, (int) payloadSize, cancellationToken);
                            await targetWriter.FlushAsync();
                            break;
                        case FrameType.ContentHeader:
                            await socket.FillBufferAsync(contentHeaderCustomHeader);

                            ((ReadOnlySpan<byte>) (contentHeaderCustomHeader
                                .Slice(4) // skip 2 obsolete shorts
                                .Span)).Read(out ulong contentSide);

                            var headerSize = payloadSize - customHeaderSize;
                            ChannelFrame.WriteHeader(targetWriter, ChannelFrameType.Content, sizeof(uint) + headerSize + (uint) contentSide);
                            targetWriter.GetMemory(sizeof(int)).Span.Write((int) headerSize);
                            targetWriter.Advance(sizeof(int));
                            await socket.FillBufferAsync(targetWriter, (int) headerSize);
                            break;
                        case FrameType.ContentBody:
                            await socket.FillBufferAsync(targetWriter, (int) payloadSize, cancellationToken);
                            await targetWriter.FlushAsync();
                            break;
                    }

                    // 4. Ensure there is FrameEnd
                    await socket.FillBufferAsync(endFrameBuffer);

                    if (endFrameBuffer[0] != ProtocolConstants.FrameEndByte)
                    {
                        // TODO: throw connection exception here
                        throw new InvalidOperationException();
                    }
                }
            }
            catch (SocketException ex)
            {
                // todo: report to diagnostic source

                this.ConnectionClose(ex);
            }
        }

        private void ConnectionClose(Exception ex)
        {
            if (this.socketIoCancellation == null || this.socketIoCancellation.IsCancellationRequested)
            {
                return;
            }

            this.State = ConnectionState.Closed;
            this.socketIoCancellation.Cancel();
            this.frameSender?.Dispose();

            this.channelPool.ReleaseAll(ex);
        }

        private static async Task<(ushort ChannelMax, uint MaxFrameSize, ushort HeartbeatInterval)> NegotiateAsync(
            ISynchronizedChannel channel, string vhost, string locale, IAuthMechanism auth, IReadOnlyDictionary<string, object> clientProperties,
            CancellationToken cancellation)
        {
            var startMethodTask = channel.WaitAsync<StartMethod>(cancellation);
            var startMethod = await startMethodTask;

            if (!startMethod.Mechanisms.Contains(auth.Type))
            {
                throw new NotSupportedException("Provided auth mechanism does not supported by the server");
            }

            var tuneMethodTask = channel.WaitAsync<TuneMethod>(cancellation);
            await channel.SendAsync(new StartOkMethod(auth.Type, auth.ToResponse(), locale, clientProperties));

            var tuneMethod = await tuneMethodTask;

            await channel.SendAsync(new TuneOkMethod(tuneMethod.ChannelMax, tuneMethod.MaxFrameSize, tuneMethod.HeartbeatInterval));

            // todo: handle wrong vhost name
            await channel.SendAsync<OpenMethod, OpenOkMethod>(new OpenMethod(vhost), cancellation);

            return (tuneMethod.ChannelMax, tuneMethod.MaxFrameSize, tuneMethod.HeartbeatInterval);
        }
    }
}