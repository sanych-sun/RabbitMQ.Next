using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Exceptions;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Buffers;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Sockets;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods.Connection;
using Channel = RabbitMQ.Next.Channels.Channel;

namespace RabbitMQ.Next
{
    internal class Connection : IConnection
    {
        private static readonly StaticMemoryBlock AmqpProtocolHeader = new StaticMemoryBlock(ProtocolConstants.AmqpHeader);
        private static readonly StaticMemoryBlock AmqpHeartbeatFrame = new StaticMemoryBlock(ProtocolConstants.HeartbeatFrame);

        private readonly IMethodRegistry methodRegistry;

        private readonly ChannelPool channelPool;
        private readonly IBufferPool bufferPool;
        private readonly ConnectionDetails connectionDetails = new ConnectionDetails();

        private Channel<IMemoryOwner<byte>> socketChannel;
        private CancellationTokenSource socketIoCancellation;

        private Connection(IMethodRegistry methodRegistry, IBufferPool bufferPool)
        {
            this.methodRegistry = methodRegistry;

            this.State = ConnectionState.Pending;
            this.bufferPool = bufferPool;
            this.channelPool = new ChannelPool();
        }

        public static async Task<IConnection> ConnectAsync(IReadOnlyList<Endpoint> endpoints,
            string virtualHost,
            IAuthMechanism authMechanism,
            string locale,
            IReadOnlyDictionary<string, object> clientProperties,
            IMethodRegistry methodRegistry,
            int frameSize)
        {
            var bufferSize = ProtocolConstants.FrameHeaderSize + frameSize + 1; // frame header + frame + frame-end
            var bufferPool = new BufferPool(bufferSize, 100);

            var connection = new Connection(methodRegistry, bufferPool);
            // TODO: adopt authentication_failure_close capability to handle auth errors

            connection.State = ConnectionState.Connecting;
            var socket = await OpenSocketAsync(endpoints);
            connection.socketChannel = System.Threading.Channels.Channel.CreateBounded<IMemoryOwner<byte>>(new BoundedChannelOptions(100)
            {
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = false,
            });

            connection.State = ConnectionState.Negotiating;
            var connectionChannel = new Channel(connection.channelPool, connection.methodRegistry, connection.socketChannel.Writer, connection.bufferPool, null, ProtocolConstants.FrameMinSize);

            connection.socketIoCancellation = new CancellationTokenSource();
            Task.Factory.StartNew(() => connection.ReceiveLoop(socket, connection.socketIoCancellation.Token), TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(() => SendLoop(socket, connection.socketChannel.Reader), TaskCreationOptions.LongRunning);

            // connection should be forcibly closed if negotiation phase take more then 10s.
            var cancellation = new CancellationTokenSource(10000);
            var negotiateTask = NegotiateAsync(connectionChannel, virtualHost, locale, authMechanism, frameSize, clientProperties, cancellation.Token);
            await connection.socketChannel.Writer.WriteAsync(AmqpProtocolHeader);

            var negotiationResults = await negotiateTask;

            var heartbeatIntervalMs = negotiationResults.HeartbeatInterval * 1000;
            // set buffer size twice as frame size, so the most messages (method + header + content) will fit into single buffer
            // connection.bufferManager.SetBufferSize(2 * (int)negotiationResults.MaxFrameSize);

            connection.connectionDetails.HeartbeatInterval = negotiationResults.HeartbeatInterval;
            connection.connectionDetails.FrameMaxSize = negotiationResults.FrameSize;

            connectionChannel.OnCompleted(connection.ConnectionClose);

            // start heartbeat
            Task.Factory.StartNew(() => connection.HeartbeatLoop(heartbeatIntervalMs, connection.socketIoCancellation.Token), TaskCreationOptions.LongRunning);

            connection.State = ConnectionState.Configuring;
            connection.State = ConnectionState.Open;

            return connection;
        }

        public ConnectionState State { get; private set; }

        public IConnectionDetails Details => this.connectionDetails;

        public IMethodRegistry MethodRegistry => this.methodRegistry;

        public async Task<IChannel> OpenChannelAsync(IReadOnlyList<IMethodHandler> handlers = null, CancellationToken cancellationToken = default)
        {
            // TODO: validate state

            var channel = new Channel(this.channelPool, this.methodRegistry, this.socketChannel.Writer, this.bufferPool, handlers, this.Details.FrameMaxSize);
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
                await this.socketChannel.Writer.WriteAsync(AmqpHeartbeatFrame);
            }
        }

        private void ReceiveLoop(ISocket socket, CancellationToken cancellationToken)
        {
            var headerBuffer = new byte[ProtocolConstants.FrameHeaderSize];

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // 1. Read frame header
                    socket.FillBuffer(headerBuffer);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    ((ReadOnlyMemory<byte>) headerBuffer).ReadFrameHeader(out FrameType frameType, out ushort channel, out uint payloadSize);

                    // 2. Get buffer
                    var buffer = this.bufferPool.CreateMemory();

                    // 3. Read payload into the buffer, allocate extra byte for FrameEndByte
                    socket.FillBuffer(buffer.Memory[0..((int)payloadSize + 1)]);

                    // 4. Ensure there is FrameEnd at last position
                    if (buffer.Memory.Span[(int)payloadSize] != ProtocolConstants.FrameEndByte)
                    {
                        // TODO: throw connection exception here
                        throw new InvalidOperationException();
                    }

                    // 5. Shrink buffer to the data size
                    buffer.Slice((int)payloadSize);

                    // 5. Write frame to appropriate channel
                    var targetChannel = this.channelPool[channel].FrameWriter;
                    if (!targetChannel.TryWrite((frameType, buffer)))
                    {
                        // should never get here, as channel suppose to be unbounded.
                        throw new InvalidOperationException("Cannot write frame into the target channel");
                    }
                }
            }
            catch (SocketException ex)
            {
                // todo: report to diagnostic source

                this.ConnectionClose(ex);
            }
        }

        private static async Task SendLoop(ISocket socket, ChannelReader<IMemoryOwner<byte>> socketChannel)
        {
            while (await socketChannel.WaitToReadAsync())
            {
                while (socketChannel.TryRead(out var memoryBlock))
                {
                    socket.Send(memoryBlock.Memory);
                    memoryBlock.Dispose();
                }
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
            this.socketChannel.Writer.TryComplete();

            this.channelPool.ReleaseAll(ex);
        }

        private static async Task<(ushort ChannelMax, int FrameSize, ushort HeartbeatInterval)> NegotiateAsync(
            IChannel channel, string vhost, string locale, IAuthMechanism auth,
            int frameSize, IReadOnlyDictionary<string, object> clientProperties,
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

            frameSize = Math.Min(frameSize, (int)tuneMethod.MaxFrameSize);
            await channel.SendAsync(new TuneOkMethod(tuneMethod.ChannelMax, (uint)frameSize, tuneMethod.HeartbeatInterval));

            // todo: handle wrong vhost name
            await channel.SendAsync<OpenMethod, OpenOkMethod>(new OpenMethod(vhost), cancellation);

            return (tuneMethod.ChannelMax, frameSize, tuneMethod.HeartbeatInterval);
        }
    }
}