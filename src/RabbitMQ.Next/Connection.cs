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
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods.Connection;
using Channel = RabbitMQ.Next.Channels.Channel;

namespace RabbitMQ.Next;

internal class Connection : IConnection
{
    private readonly SemaphoreSlim connectionStateLock = new (1, 1);
    private readonly ChannelPool channelPool;
    private readonly ConnectionDetails connectionDetails;
    private readonly Channel<IMemoryAccessor> socketSender;
    private readonly ObjectPool<byte[]> memoryPool;

    private ISocket socket;
    private CancellationTokenSource socketIoCancellation;
    private IChannelInternal connectionChannel;

    public Connection(ConnectionSettings settings)
    {
        // for best performance and code simplification buffer should fit entire frame
        // (frame header + frame payload + frame-end)
        var bufferSize = ProtocolConstants.FrameHeaderSize + settings.MaxFrameSize + ProtocolConstants.FrameEndSize;
        this.memoryPool = new DefaultObjectPool<byte[]>(new MemoryPoolPolicy(bufferSize), 100);
        
        this.connectionDetails = new ConnectionDetails(settings);
        this.socketSender = System.Threading.Channels.Channel.CreateBounded<IMemoryAccessor>(new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false,
        });
            
        this.State = ConnectionState.Closed;
        this.channelPool = new ChannelPool(this.CreateChannel);
    }

    public ConnectionState State { get; private set; }

    public async Task OpenAsync(CancellationToken cancellation = default)
    {
        await this.EnsureConnectionOpenAsync(cancellation).ConfigureAwait(false);
    }
    
    public async Task<IChannel> OpenChannelAsync(CancellationToken cancellation = default)
    {
        await this.EnsureConnectionOpenAsync(cancellation).ConfigureAwait(false);

        var channel = this.channelPool.Create();
        await channel.SendAsync<Transport.Methods.Channel.OpenMethod, Transport.Methods.Channel.OpenOkMethod>(new Transport.Methods.Channel.OpenMethod(), cancellation).ConfigureAwait(false);
            
        return channel;
    }

    public async ValueTask DisposeAsync()
    {
        if (this.State == ConnectionState.Open)
        {
            await this.connectionChannel.SendAsync<CloseMethod, CloseOkMethod>(new CloseMethod((ushort)ReplyCode.Success, "Goodbye", 0)).ConfigureAwait(false);

            this.ConnectionClose(null);
        }

        this.socketIoCancellation?.Dispose();   
    }
        
    public async Task OpenConnectionAsync(CancellationToken cancellation)
    {
        // todo: validate state here

        this.socket = await EndpointResolver.OpenSocketAsync(this.connectionDetails.Settings.Endpoints, cancellation).ConfigureAwait(false);

        this.socketIoCancellation = new CancellationTokenSource();
        StartThread(() => this.ReceiveLoop(this.socketIoCancellation.Token), "RabbitMQ.Next-Receive loop");
        StartThread(this.SendLoop, "RabbitMQ.Next-Send loop");

        this.connectionChannel = this.CreateChannel(ProtocolConstants.ConnectionChannel);
        var connectionCloseWait = new WaitMethodMessageHandler<CloseMethod>(default);
        connectionCloseWait.WaitTask.ContinueWith(t =>
        {
            if (t.IsCompleted && !this.connectionChannel.Completion.IsCompleted)
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
        var amqpHeaderMemory = new MemoryAccessor(ProtocolConstants.AmqpHeader);
        await this.socketSender.Writer.WriteAsync(amqpHeaderMemory, cancellation).ConfigureAwait(false);

        var negotiationResults = await negotiateTask.ConfigureAwait(false);
        this.connectionDetails.PopulateWithNegotiationResults(negotiationResults);
        
        this.State = ConnectionState.Configuring;
        this.State = ConnectionState.Open;
    }

    private IChannelInternal CreateChannel(ushort channelNumber)
    {
        var maxFrameSize = this.connectionDetails.FrameMaxSize ?? ProtocolConstants.FrameMinSize;
        var policy = new MessageBuilderPoolPolicy(this.memoryPool, channelNumber, maxFrameSize);
        var messageBuilderPool = new DefaultObjectPool<MessageBuilder>(policy);

        return new Channel(this.socketSender.Writer, messageBuilderPool, this.connectionDetails.Settings.Serializer);
    }

    private void SendLoop()
    {
        var heartbeatMemory = new MemoryAccessor(ProtocolConstants.HeartbeatFrame);
        var socketChannel = this.socketSender.Reader;
        
        do
        {
            while (socketChannel.TryRead(out var memory))
            {
                this.socket.Send(memory);

                while(memory != null)
                {
                    var next = memory.Next;
                    memory.Dispose();
                    memory = next;
                }
            }

            var waitTask = socketChannel.WaitToReadAsync().AsTask();
            if (waitTask.Wait(this.connectionDetails.HeartbeatInterval ?? TimeSpan.FromMilliseconds(-1)))
            {
                if (!waitTask.Result)
                {
                    return;
                }
            }
            else
            {
                // wait long enough and nothing was sent, need to send heartbeat frame
                this.socket.Send(heartbeatMemory);    
            }
        } while (true);
    }

    private void ReceiveLoop(CancellationToken cancellationToken)
    {
        try
        {
            // 1. Receive next chunk of data.
            SharedMemory receivedMemory = null;

            while (!cancellationToken.IsCancellationRequested)
            {
                SharedMemory.MemoryAccessor currentAccessor = default;
                if (receivedMemory != null)
                {
                    currentAccessor = receivedMemory;
                }

                // 2. Parse received frames
                while (currentAccessor.Size >= ProtocolConstants.FrameHeaderSize)
                {
                    // 2.1. Read frame header
                    currentAccessor.Span.ReadFrameHeader(out var frameType, out var channel, out var payloadSize);
                    currentAccessor = currentAccessor.Slice(ProtocolConstants.FrameHeaderSize);

                    // 2.2. Lookup for the target channel to push the frame
                    var targetChannel = channel == ProtocolConstants.ConnectionChannel ? this.connectionChannel : this.channelPool.Get(channel);

                    // 2.3. Slice frame bytes
                    SharedMemory.MemoryAccessor frameBytes;
                    if (currentAccessor.Size >= payloadSize + ProtocolConstants.FrameEndSize)
                    {
                        frameBytes = currentAccessor.Slice(0, (int)payloadSize);
                    }
                    else
                    {
                        var missedBytes = (int)payloadSize - currentAccessor.Size;

                        if (frameType == FrameType.ContentBody)
                        {
                            // ContentBody frame could be easily processed chunked
                            targetChannel.PushFrame(frameType, currentAccessor);
                            currentAccessor = default;
                        }

                        var previousMemory = receivedMemory;
                        receivedMemory = ReceiveNext(missedBytes + ProtocolConstants.FrameEndSize, currentAccessor);
                        previousMemory.Dispose();

                        currentAccessor = receivedMemory;
                        frameBytes = currentAccessor.Slice(0, frameType == FrameType.ContentBody ? missedBytes : (int)payloadSize);
                    }

                    // 2.4. Ensure frame end present just after the current frame bytes
                    if (currentAccessor.Span[frameBytes.Size] != ProtocolConstants.FrameEndByte)
                    {
                        // TODO: throw connection exception here
                        throw new InvalidOperationException();
                    }

                    // 2.5. Push frame to the target channel
                    targetChannel.PushFrame(frameType, frameBytes);
                    currentAccessor = currentAccessor.Slice(frameBytes.Size + ProtocolConstants.FrameEndSize);
                }

                // 3. Receive next chunk with preserving leftovers of the currently not yet parsed data
                var nextChunk = ReceiveNext(ProtocolConstants.FrameHeaderSize, currentAccessor);
                receivedMemory?.Dispose();
                receivedMemory = nextChunk;
            }
        }
        catch (SocketException ex)
        {
            // todo: report to diagnostic source

            this.ConnectionClose(ex);
        }
        catch (Exception ex)
        {
            this.ConnectionClose(ex);
        }

        return;
        
        SharedMemory ReceiveNext(int expectedBytes, SharedMemory.MemoryAccessor previousChunk = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            // Obtain next buffer
            var buffer = this.memoryPool.Get();
            var bufferOffset = 0;
                
            // Copy bytes leftover from previous chunk if any
            if (previousChunk.Size > 0)
            {
                previousChunk.Span.CopyTo(buffer);
                bufferOffset = previousChunk.Size;
                expectedBytes -= bufferOffset;
            }
                
            // Read data from the socket at least of requested size
            var received = this.socket.Receive(buffer, bufferOffset, expectedBytes);
            return new SharedMemory(this.memoryPool, buffer, bufferOffset + received);
        }
    }

    private async ValueTask EnsureConnectionOpenAsync(CancellationToken cancellation)
    {
        if (this.State == ConnectionState.Open)
        {
            return;
        }

        await this.connectionStateLock.WaitAsync(cancellation).ConfigureAwait(false);
        try
        {
            if (this.State == ConnectionState.Open)
            {
                return;
            }

            await this.OpenConnectionAsync(cancellation).ConfigureAwait(false);
        }
        finally
        {
            this.connectionStateLock.Release();
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
        
        this.socket.Dispose();
    }

    private static void StartThread(Action threadStart, string threadName)
    {
        var thread = new Thread(new ThreadStart(threadStart))
        {
            Name = threadName,
        };
        thread.Start();
    }

    private static async Task<NegotiationResults> NegotiateConnectionAsync(IChannel channel, ConnectionSettings settings, CancellationToken cancellation)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        // connection should be forcibly closed if negotiation phase take more than 10s.
        cts.CancelAfter(TimeSpan.FromSeconds(10));
        cancellation = cts.Token;

        var startMethod = await channel.WaitAsync<StartMethod>(cancellation).ConfigureAwait(false);
        if (!startMethod.Mechanisms.Contains(settings.Auth.Type))
        {
            throw new NotSupportedException("Provided auth mechanism does not supported by the server");
        }

        var saslStartBytes = await settings.Auth.StartAsync().ConfigureAwait(false);

        var tuneMethodTask = channel.WaitAsync<TuneMethod>(cancellation);
        var secureMethodTask = channel.WaitAsync<SecureMethod>(cancellation);

        await channel.SendAsync(new StartOkMethod(settings.Auth.Type, saslStartBytes, settings.Locale, settings.ClientProperties), cancellation).ConfigureAwait(false);
        
        do
        {
            await Task.WhenAny(tuneMethodTask, secureMethodTask).ConfigureAwait(false);

            if (secureMethodTask.IsCompleted)
            {
                var secureRequest = await secureMethodTask.ConfigureAwait(false);
                var secureResponse = await settings.Auth.HandleChallengeAsync(secureRequest.Challenge.Span).ConfigureAwait(false);
                
                // wait for another secure round-trip just in case
                secureMethodTask = channel.WaitAsync<SecureMethod>(cancellation);
                await channel.SendAsync(new SecureOkMethod(secureResponse), cancellation).ConfigureAwait(false);
            }
            
        } while (!tuneMethodTask.IsCompleted);

        var tuneMethod = await tuneMethodTask.ConfigureAwait(false);
        var negotiationResult = new NegotiationResults(
            tuneMethod.ChannelMax,
            Math.Min(settings.MaxFrameSize, (int)tuneMethod.MaxFrameSize),
            TimeSpan.FromSeconds(tuneMethod.HeartbeatInterval));

        await channel.SendAsync(new TuneOkMethod(tuneMethod.ChannelMax, (uint)negotiationResult.FrameMaxSize, tuneMethod.HeartbeatInterval), cancellation).ConfigureAwait(false);

        // todo: handle wrong vhost name
        await channel.SendAsync<OpenMethod, OpenOkMethod>(new OpenMethod(settings.Vhost), cancellation).ConfigureAwait(false);

        return negotiationResult;
    }
}
