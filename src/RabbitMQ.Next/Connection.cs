using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Next.Exceptions;
using RabbitMQ.Next.Buffers;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Sockets;
using RabbitMQ.Next.Tasks;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Messaging;
using RabbitMQ.Next.Transport.Methods.Connection;
using RabbitMQ.Next.Transport.Methods.Registry;
using Channel = RabbitMQ.Next.Channels.Channel;

namespace RabbitMQ.Next;

internal class Connection : IConnectionInternal
{
    private readonly ChannelPool channelPool;
    private readonly ConnectionDetails connectionDetails;
    private readonly Channel<MemoryBlock> socketSender;

    private ISocket socket;
    private CancellationTokenSource socketIoCancellation;
    private IChannelInternal connectionChannel;

    public Connection(ConnectionSettings settings, IMethodRegistry methodRegistry, ObjectPool<MemoryBlock> memoryPool, ObjectPool<FrameBuilder> frameBuilderPool)
    {
        this.connectionDetails = new ConnectionDetails(settings);
        this.MethodRegistry = methodRegistry;
        this.MessagePropertiesPool = new DefaultObjectPool<LazyMessageProperties>(new LazyMessagePropertiesPolicy());
        this.socketSender = System.Threading.Channels.Channel.CreateBounded<MemoryBlock>(new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false,
        });
            
        this.State = ConnectionState.Pending;
        this.MemoryPool = memoryPool;
        this.FrameBuilderPool = frameBuilderPool;
        this.channelPool = new ChannelPool(num => new Channel(this, num, this.connectionDetails.Negotiated.FrameMaxSize));
    }

    public ConnectionState State { get; private set; }

    public IMethodRegistry MethodRegistry { get; }

    public ObjectPool<MemoryBlock> MemoryPool { get; }
        
    public ObjectPool<LazyMessageProperties> MessagePropertiesPool { get; }

    public ObjectPool<FrameBuilder> FrameBuilderPool { get; }

    public ValueTask WriteToSocketAsync(MemoryBlock memory, CancellationToken cancellation = default)
    {
        if (this.socketSender.Writer.TryWrite(memory))
        {
            return default;
        }

        return this.socketSender.Writer.WriteAsync(memory, cancellation);
    }

    public async Task<IChannel> OpenChannelAsync(CancellationToken cancellationToken = default)
    {
        // TODO: validate state

        var channel = this.channelPool.Create();
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

        this.socketIoCancellation = new CancellationTokenSource();
        Task.Factory.StartNew(() => this.ReceiveLoop(this.socketIoCancellation.Token), TaskCreationOptions.LongRunning);
        Task.Factory.StartNew(this.SendLoop, TaskCreationOptions.LongRunning);

        this.connectionChannel = new Channel(this, ProtocolConstants.ConnectionChannel, ProtocolConstants.FrameMinSize);
        var connectionCloseWait = new WaitMethodMessageHandler<CloseMethod>();
        connectionCloseWait.WaitTask.ContinueWith(t =>
        {
            if (t.IsCompleted)
            {
                this.connectionChannel.TryComplete(new ConnectionException(t.Result.StatusCode, t.Result.Description));
            }
        });

        this.connectionChannel.WithMessageHandler(connectionCloseWait);
        this.connectionChannel.Completion.ContinueWith(t =>
        {
            var ex = t.Exception?.InnerException ?? t.Exception;
            this.ConnectionClose(ex);
        });

        this.State = ConnectionState.Connecting;
        // TODO: adopt authentication_failure_close capability to handle auth errors

        var negotiateTask = NegotiateConnectionAsync(this.connectionChannel, this.connectionDetails.Settings, cancellation);
        await this.WriteToSocketAsync(ProtocolConstants.AmqpHeader);

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
            await this.WriteToSocketAsync(ProtocolConstants.HeartbeatFrame);
        }
    }

    private async Task WriteToSocketAsync(ReadOnlyMemory<byte> bytes)
    {
        var memoryBlock = this.MemoryPool.Get();
        memoryBlock.Write(bytes.Span);

        // Should not return memory block here, it will be done in SendLoop
        await this.WriteToSocketAsync(memoryBlock);
    }

    private async Task SendLoop()
    {
        var socketChannel = this.socketSender.Reader;
        while (await socketChannel.WaitToReadAsync())
        {
            while (socketChannel.TryRead(out var memoryBlock))
            {
                var current = memoryBlock;
                while (current != null)
                {
                    this.socket.Send(current.Memory);
                    current = current.Next;
                }
                
                this.socket.Flush();
                this.MemoryPool.Return(memoryBlock);
            }
        }
    }

    private void ReceiveLoop(CancellationToken cancellationToken)
    {
        var headerBuffer = new ArraySegment<byte>(new byte[ProtocolConstants.FrameHeaderSize]);

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

                ((ReadOnlySpan<byte>)headerBuffer).ReadFrameHeader(out FrameType frameType, out ushort channel, out uint payloadSize);

                // 2. Get buffer
                var buffer = this.MemoryPool.Get();

                // 3. Read payload into the buffer, allocate extra byte for FrameEndByte
                var payload = buffer.Memory[..((int)payloadSize + 1)];
                this.socket.FillBuffer(payload);
                // 4. Ensure there is FrameEnd
                if (payload[(int)payloadSize] != ProtocolConstants.FrameEndByte)
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
                buffer.Slice((int)payloadSize);

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
        this.socketSender.Writer.TryComplete();

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
            (int)tuneMethod.MaxFrameSize,
            TimeSpan.FromSeconds(tuneMethod.HeartbeatInterval));

        await channel.SendAsync(new TuneOkMethod(tuneMethod.ChannelMax, (uint)negotiationResult.FrameMaxSize, tuneMethod.HeartbeatInterval), cancellation);

        // todo: handle wrong vhost name
        await channel.SendAsync<OpenMethod, OpenOkMethod>(new OpenMethod(settings.Vhost), cancellation);

        return negotiationResult;
    }
}