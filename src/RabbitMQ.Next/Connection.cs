using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Exceptions;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Buffers;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Sockets;
using RabbitMQ.Next.Tasks;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods.Connection;
using Channel = RabbitMQ.Next.Channels.Channel;

namespace RabbitMQ.Next
{
    internal class Connection : IConnectionInternal
    {
        private readonly ChannelPool channelPool;
        private readonly ConnectionDetails connectionDetails;

        private ISocket socket;
        private CancellationTokenSource socketIoCancellation;
        private IChannelInternal connectionChannel;

        public Connection(ConnectionSettings settings, IMethodRegistry methodRegistry, ObjectPool<MemoryBlock> memoryPool, ObjectPool<FrameBuilder> frameBuilderPool)
        {
            this.connectionDetails = new ConnectionDetails(settings);
            this.MethodRegistry = methodRegistry;

            this.State = ConnectionState.Pending;
            this.MemoryPool = memoryPool;
            this.FrameBuilderPool = frameBuilderPool;
            this.channelPool = new ChannelPool(num => new Channel(this, num, this.connectionDetails.Negotiated.FrameMaxSize));
        }

        public ConnectionState State { get; private set; }

        public IMethodRegistry MethodRegistry { get; }

        public ChannelWriter<MemoryBlock> SocketWriter { get; private set; }

        public ObjectPool<MemoryBlock> MemoryPool { get; }

        public ObjectPool<FrameBuilder> FrameBuilderPool { get; }

        public async Task<IChannel> OpenChannelAsync(IReadOnlyList<IFrameHandler> handlers = null, CancellationToken cancellationToken = default)
        {
            // TODO: validate state

            var channel = this.channelPool.Create();
            if (handlers != null)
            {
                foreach (var h in handlers)
                {
                    channel.AddFrameHandler(h);
                }
            }

            await channel.SendAsync<Transport.Methods.Channel.OpenMethod, Transport.Methods.Channel.OpenOkMethod>(new Transport.Methods.Channel.OpenMethod(), cancellationToken);

            return channel;
        }

        public async ValueTask DisposeAsync()
        {
            if (this.State != ConnectionState.Open)
            {
                return;
            }

            await this.connectionChannel.SendAsync<CloseMethod, CloseOkMethod>(new CloseMethod((ushort)ReplyCode.Success, "Goodbye", 0));

            this.ConnectionClose(null);
        }
        
        public async Task OpenConnectionAsync(CancellationToken cancellation)
        {
            // todo: validate state here

            this.socket = await EndpointResolver.OpenSocketAsync(this.connectionDetails.Settings.Endpoints, cancellation);

            var socketChannel = System.Threading.Channels.Channel.CreateBounded<MemoryBlock>(new BoundedChannelOptions(100)
            {
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = false,
            });

            this.SocketWriter = socketChannel.Writer;
            this.socketIoCancellation = new CancellationTokenSource();
            Task.Factory.StartNew(() => ReceiveLoop(this.socketIoCancellation.Token), TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(() => SendLoop(socketChannel.Reader), TaskCreationOptions.LongRunning);

            this.connectionChannel = new Channel(this, ProtocolConstants.ConnectionChannel, ProtocolConstants.FrameMinSize);
            var connectionCloseWait = new WaitMethodFrameHandler<CloseMethod>(this.MethodRegistry);
            connectionCloseWait.WaitTask.ContinueWith(t =>
            {
                if (t.IsCompleted)
                {
                    this.connectionChannel.TryComplete(new ConnectionException(t.Result.StatusCode, t.Result.Description));
                }
            });

            this.connectionChannel.AddFrameHandler(connectionCloseWait);
            this.connectionChannel.Completion.ContinueWith(t =>
            {
                var ex = t.Exception?.InnerException ?? t.Exception;
                this.ConnectionClose(ex);
            });

            this.State = ConnectionState.Connecting;
            // TODO: adopt authentication_failure_close capability to handle auth errors

            var negotiateTask = NegotiateConnectionAsync(this.connectionChannel, this.connectionDetails.Settings, cancellation);
            await this.WriteToSocket(ProtocolConstants.AmqpHeader);

            this.connectionDetails.Negotiated = await negotiateTask;

            // start heartbeat
            Task.Factory.StartNew(() => this.HeartbeatLoop(this.connectionDetails.Negotiated.HeartbeatInterval, this.socketIoCancellation.Token), TaskCreationOptions.LongRunning);

            this.State = ConnectionState.Configuring;
            this.State = ConnectionState.Open;
        }

        private async Task HeartbeatLoop(TimeSpan interval, CancellationToken cancellation)
        {
            while (!cancellation.IsCancellationRequested)
            {
                await Task.Delay(interval, cancellation);
                await this.WriteToSocket(ProtocolConstants.HeartbeatFrame);
            }
        }

        private ValueTask WriteToSocket(ReadOnlyMemory<byte> bytes)
        {
            var memoryBlock = this.MemoryPool.Get();

            bytes.CopyTo(memoryBlock.Writer);
            memoryBlock.Commit(ProtocolConstants.AmqpHeader.Length);

            // Should not return memory block here, it will be done in SendLoop
            return this.SocketWriter.WriteAsync(memoryBlock);
        }

        private async Task SendLoop(ChannelReader<MemoryBlock> socketChannel)
        {
            while (await socketChannel.WaitToReadAsync())
            {
                while (socketChannel.TryRead(out var memoryBlock))
                {
                    await this.socket.SendAsync(memoryBlock.Memory);
                    this.MemoryPool.Return(memoryBlock);
                }

                await this.socket.FlushAsync();
            }
        }

        private void ReceiveLoop(CancellationToken cancellationToken)
        {
            Memory<byte> headerBuffer = new byte[ProtocolConstants.FrameHeaderSize];

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // 1. Read frame header
                    this.socket.FillBuffer(headerBuffer);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    ((ReadOnlyMemory<byte>) headerBuffer).ReadFrameHeader(out FrameType frameType, out ushort channel, out uint payloadSize);

                    // 2. Get buffer
                    var buffer = this.MemoryPool.Get();

                    // 3. Read payload into the buffer, allocate extra byte for FrameEndByte
                    var payload = buffer.Writer[..((int)payloadSize + 1)];
                    this.socket.FillBuffer(payload);
                    // 4. Ensure there is FrameEnd
                    if (payload.Span[(int)payloadSize] != ProtocolConstants.FrameEndByte)
                    {
                        // TODO: throw connection exception here
                        throw new InvalidOperationException();
                    }

                    // 5. Doing nothing on heartbeat frame
                    if (frameType == FrameType.Heartbeat)
                    {
                        this.MemoryPool.Return(buffer);
                        continue;
                    }

                    // 6. Shrink the buffer to the payload size
                    buffer.Commit((int)payloadSize);

                    // 7. Write frame to appropriate channel
                    var targetChannel = (channel == ProtocolConstants.ConnectionChannel) ? this.connectionChannel.FrameWriter: this.channelPool.Get(channel).FrameWriter;
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

        private void ConnectionClose(Exception ex)
        {
            if (this.socketIoCancellation == null || this.socketIoCancellation.IsCancellationRequested)
            {
                return;
            }

            this.State = ConnectionState.Closed;
            this.socketIoCancellation.Cancel();
            this.SocketWriter.TryComplete();

            this.channelPool.ReleaseAll(ex);
            this.connectionChannel.TryComplete(ex);
        }

        private static async Task<NegotiationResults> NegotiateConnectionAsync(IChannel channel, ConnectionSettings settings, CancellationToken cancellation)
        {
            // connection should be forcibly closed if negotiation phase take more then 10s.
            cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token.Combine(cancellation);

            var startMethodTask = channel.WaitAsync<StartMethod>(cancellation);

            var startMethod = await startMethodTask;

            if (!startMethod.Mechanisms.Contains(settings.Auth.Type))
            {
                throw new NotSupportedException("Provided auth mechanism does not supported by the server");
            }

            var tuneMethodTask = channel.WaitAsync<TuneMethod>(cancellation);
            await channel.SendAsync(new StartOkMethod(settings.Auth.Type, settings.Auth.ToResponse(), settings.Locale, settings.ClientProperties), cancellation);

            var tuneMethod = await tuneMethodTask;
            var negotiationResult = new NegotiationResults(
                settings.Auth.Type,
                tuneMethod.ChannelMax,
                Math.Min(settings.MaxFrameSize, (int)tuneMethod.MaxFrameSize),
                TimeSpan.FromSeconds(tuneMethod.HeartbeatInterval));

            await channel.SendAsync(new TuneOkMethod(tuneMethod.ChannelMax, (uint)negotiationResult.FrameMaxSize, tuneMethod.HeartbeatInterval), cancellation);

            // todo: handle wrong vhost name
            await channel.SendAsync<OpenMethod, OpenOkMethod>(new OpenMethod(settings.Vhost), cancellation);

            return negotiationResult;
        }
    }
}