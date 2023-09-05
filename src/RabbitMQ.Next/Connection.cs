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
using RabbitMQ.Next.Transport.Methods.Connection;
using Channel = RabbitMQ.Next.Channels.Channel;

namespace RabbitMQ.Next;

internal class Connection : IConnection
{
    private readonly ChannelPool channelPool;
    private readonly ConnectionDetails connectionDetails;
    private readonly Channel<MemoryBlock> socketSender;
    private readonly ObjectPool<byte[]> memoryPool;

    private ISocket socket;
    private CancellationTokenSource socketIoCancellation;
    private IChannelInternal connectionChannel;

    public Connection(ConnectionSettings settings, ObjectPool<MemoryBlock> memoryPool2, ObjectPool<byte[]> memoryPool)
    {
        this.connectionDetails = new ConnectionDetails(settings);
        this.socketSender = System.Threading.Channels.Channel.CreateBounded<MemoryBlock>(new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false,
        });
            
        this.State = ConnectionState.Closed;
        this.MemoryPool2 = memoryPool2;
        this.memoryPool = memoryPool;
        this.channelPool = new ChannelPool(num => new Channel(this.socketSender.Writer, this.MemoryPool2, num, this.connectionDetails.Negotiated.FrameMaxSize));
    }

    public ConnectionState State { get; private set; }

    public ObjectPool<MemoryBlock> MemoryPool2 { get; }

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

        this.connectionChannel = new Channel(this.socketSender.Writer, this.MemoryPool2, ProtocolConstants.ConnectionChannel, ProtocolConstants.FrameMinSize);
        var connectionCloseWait = new WaitMethodMessageHandler<CloseMethod>(default);
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

    private ValueTask WriteToSocketAsync(ReadOnlyMemory<byte> bytes)
    {
        var memoryBlock = this.MemoryPool2.Get();
        memoryBlock.Write(bytes.Span);

        if (this.socketSender.Writer.TryWrite(memoryBlock))
        {
            return default;
        }
        
        // Should not return memory block here, it will be done in SendLoop
        return this.socketSender.Writer.WriteAsync(memoryBlock);
    }

    private async Task SendLoop()
    {
        var socketChannel = this.socketSender.Reader;
        while (await socketChannel.WaitToReadAsync())
        {
            while (socketChannel.TryRead(out var memoryBlock))
            {
                this.socket.Send(memoryBlock);
                this.MemoryPool2.Return(memoryBlock);
            }
        }
    }

    private void ReceiveLoop(CancellationToken cancellationToken)
    {
        try
        {
            IMemoryAccessor previousChunk = null;
            var expectedBytes = ProtocolConstants.FrameHeaderSize;
            while (!cancellationToken.IsCancellationRequested)
            {
                // 1. Obtain next buffer
                var buffer = this.memoryPool.Get();
                var bufferOffset = 0;
                
                // 2. Copy bytes leftover from previous chunk if any
                if (previousChunk != null)
                {
                    previousChunk.CopyTo(buffer);
                    bufferOffset = previousChunk.Size;
                    previousChunk.Dispose();
                    previousChunk = null;
                }
                
                // 3. Read data from the socket at least of frame header size
                var received = this.socket.Receive(buffer, bufferOffset, expectedBytes - bufferOffset);
                var receivedMemory = new SharedMemory(this.memoryPool, buffer);
                var bufferSize = bufferOffset + received;

                // 4. Parse received frames
                var receivedSlice = receivedMemory.Slice(0, bufferSize);
                while (receivedSlice.Length >= ProtocolConstants.FrameHeaderSize)
                {
                    // 4.1. Read frame header
                    receivedSlice.Span.ReadFrameHeader(out var frameType, out var channel, out var payloadSize);
                    var totalFrameSize = ProtocolConstants.FrameHeaderSize + (int)payloadSize + ProtocolConstants.FrameEndSize;
                    
                    // 4.2. Ensure entire frame was loaded
                    if (totalFrameSize > receivedSlice.Length)
                    {
                        expectedBytes = totalFrameSize - receivedSlice.Length;
                        break;
                    }
                    
                    // 4.3. Ensure frame end if present
                    if (receivedSlice.Span[totalFrameSize - 1] != ProtocolConstants.FrameEndByte)
                    {
                        // TODO: throw connection exception here
                        throw new InvalidOperationException();
                    }

                    // 4.4. Can safely ignore Heartbeat frame
                    if (frameType != FrameType.Heartbeat)
                    {
                        // 4.5. Slice frame payload
                        var framePayload = receivedSlice.Slice(ProtocolConstants.FrameHeaderSize, (int)payloadSize);

                        // 4.6. Write frame to appropriate channel
                        var targetChannel = (channel == ProtocolConstants.ConnectionChannel) ? this.connectionChannel : this.channelPool.Get(channel);
                        targetChannel.PushFrame(frameType, framePayload);
                    }

                    receivedSlice = receivedSlice.Slice(totalFrameSize);
                    expectedBytes = ProtocolConstants.FrameHeaderSize;
                }

                if (receivedSlice.Length > 0)
                {
                    previousChunk = receivedSlice.AsRef();
                }

                receivedMemory.Dispose();
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
            Math.Min(settings.MaxFrameSize, (int)tuneMethod.MaxFrameSize),
            TimeSpan.FromSeconds(tuneMethod.HeartbeatInterval));

        await channel.SendAsync(new TuneOkMethod(tuneMethod.ChannelMax, (uint)negotiationResult.FrameMaxSize, tuneMethod.HeartbeatInterval), cancellation);

        // todo: handle wrong vhost name
        await channel.SendAsync<OpenMethod, OpenOkMethod>(new OpenMethod(settings.Vhost), cancellation);

        return negotiationResult;
    }
}