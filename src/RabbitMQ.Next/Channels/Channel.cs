using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Next.Exceptions;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Methods;
using RabbitMQ.Next.Buffers;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Messaging;
using RabbitMQ.Next.Transport.Methods.Channel;

namespace RabbitMQ.Next.Channels;

internal sealed class Channel : IChannelInternal
{
    private readonly IMethodRegistry registry;
    private readonly ObjectPool<MemoryBlock> memoryPool;
    private readonly ObjectPool<LazyMessageProperties> messagePropertiesPool;
    private readonly MethodSender methodSender;
    private readonly TaskCompletionSource<bool> channelCompletion;
    private readonly ISerializerFactory serializerFactory;
    private readonly Dictionary<uint, IMessageProcessor> methodProcessors = new();

    public Channel(IConnectionInternal connection, ushort channelNumber, int frameMaxSize)
    {
        this.ChannelNumber = channelNumber;
        this.registry = connection.MethodRegistry;
        this.memoryPool = connection.MemoryPool;
        this.messagePropertiesPool = connection.MessagePropertiesPool;
        this.serializerFactory = connection.SerializerFactory;
        this.methodSender = new MethodSender(this.ChannelNumber, connection, frameMaxSize);

        this.channelCompletion = new TaskCompletionSource<bool>();
        var receiveChannel = System.Threading.Channels.Channel.CreateUnbounded<(FrameType Type, MemoryBlock Payload)>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true,
            AllowSynchronousContinuations = false,
        });
        this.FrameWriter = receiveChannel.Writer;

        var channelCloseWait = new WaitMethodMessageHandler<CloseMethod>();
        channelCloseWait.WaitTask.ContinueWith(t =>
        {
            if (t.IsCompletedSuccessfully)
            {
                this.TryComplete(new ChannelException(t.Result.StatusCode, t.Result.Description, t.Result.FailedMethodId));
            }
        });

        this.WithMessageHandler(channelCloseWait);

        Task.Factory.StartNew(() => this.LoopAsync(receiveChannel.Reader), TaskCreationOptions.LongRunning);
    }

    public ushort ChannelNumber { get; }

    public IDisposable WithMessageHandler<TMethod>(IMessageHandler<TMethod> handler)
        where TMethod : struct, IIncomingMethod
    {
        var methodId = (uint)this.registry.GetMethodId<TMethod>();
        if (!this.methodProcessors.TryGetValue(methodId, out var processor))
        {
            processor = new MessageProcessor<TMethod>(this.registry.GetParser<TMethod>());
            this.methodProcessors[methodId] = processor;
        }

        return processor.WithMessageHandler(handler);
    }

    public Task Completion => this.channelCompletion.Task;
    public Task SendAsync<TRequest>(TRequest request, CancellationToken cancellation = default)
        where TRequest : struct, IOutgoingMethod
    {
        this.ValidateState();
        return this.methodSender.SendAsync(request, cancellation);
    }

    public async Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellation = default)
        where TRequest : struct, IOutgoingMethod where TResponse : struct, IIncomingMethod
    {
        this.ValidateState();
        var waitTask = this.WaitAsync<TResponse>(cancellation);

        await this.methodSender.SendAsync(request, cancellation);

        return await waitTask;
    }

    public Task PublishAsync<TState>(
        TState state, string exchange, string routingKey,
        IMessageProperties properties, Action<TState, IBufferWriter<byte>> payload,
        PublishFlags flags = PublishFlags.None, CancellationToken cancellation = default)
    {
        this.ValidateState();
        return this.methodSender.PublishAsync(state, exchange, routingKey, properties, payload, flags, cancellation);
    }

    public ChannelWriter<(FrameType Type, MemoryBlock Payload)> FrameWriter { get; }

    public bool TryComplete(Exception ex = null)
    {
        if (this.channelCompletion.Task.IsCompleted)
        {
            return false;
        }

        if (ex != null)
        {
            this.channelCompletion.TrySetException(ex);
        }
        else
        {
            this.channelCompletion.TrySetResult(true);
        }

        this.FrameWriter.TryComplete();

        foreach (var processor in this.methodProcessors.Values)
        {
            processor.Release(ex);
        }
            
        this.methodProcessors.Clear();
            
        return true;
    }

    public async Task<TMethod> WaitAsync<TMethod>(CancellationToken cancellation = default)
        where TMethod : struct, IIncomingMethod
    {
        this.ValidateState();
        var waitHandler = new WaitMethodMessageHandler<TMethod>();
        var disposer = this.WithMessageHandler(waitHandler);

        var result = await waitHandler.WaitTask;
            
        disposer.Dispose();
        return result;
    }

    public async Task CloseAsync(Exception ex = null)
    {
        await this.SendAsync<CloseMethod, CloseOkMethod>(new CloseMethod((ushort) ReplyCode.Success, string.Empty, MethodId.Unknown));
        this.TryComplete(ex);
    }

    public async Task CloseAsync(ushort statusCode, string description, MethodId failedMethodId)
    {
        await this.SendAsync<CloseMethod, CloseOkMethod>(new CloseMethod(statusCode, description, failedMethodId));
        this.TryComplete(new ChannelException(statusCode, description, failedMethodId));
    }

    private async Task LoopAsync(ChannelReader<(FrameType Type, MemoryBlock Payload)> reader)
    {
        try
        {
            while (!reader.Completion.IsCompleted)
            {
                if (!reader.TryRead(out var methodFrame))
                {
                    methodFrame = await reader.ReadAsync();
                }

                // 2. Get method Id
                methodFrame.Payload.Data.Span.Read(out uint method);
                var methodId = (MethodId) method;
                        
                // 3. Get content if exists
                ContentAccessor content = null;
                   
                if (this.registry.HasContent(methodId))
                {
                    // 3.1 Get content header frame
                    if (!reader.TryRead(out var contentHeader))
                    {
                        contentHeader = await reader.ReadAsync();
                    }

                    // 3.2 Extract content body size
                    contentHeader.Payload.Data[4..] // skip 2 obsolete shorts
                        .Span.Read(out ulong contentSize);

                    MemoryBlock head = null;
                    MemoryBlock current = null;
                    long receivedContent = 0;
                    while (receivedContent < (long)contentSize)
                    {
                        if (!reader.TryRead(out var frame))
                        {
                            frame = await reader.ReadAsync();
                        }

                        if (head == null)
                        {
                            head = frame.Payload;
                            current = head;
                        }
                        else
                        {
                            current = current.Append(frame.Payload);    
                        }

                        receivedContent += frame.Payload.Data.Length;
                    }

                    content = new ContentAccessor(this.messagePropertiesPool, this.memoryPool, this.serializerFactory, contentHeader.Payload, head);
                }

                var handled = false;
                if (this.methodProcessors.TryGetValue((uint)methodId, out var processor))
                {
                    handled = processor.ProcessMessage(methodFrame.Payload.Data.Span[sizeof(uint)..], content);
                }

                // TODO: should throw on unhandled methods?
                if (!handled && content != null)
                {
                    ((IDisposable)content).Dispose();
                }
                    
                this.memoryPool.Return(methodFrame.Payload);
            }
        }
        catch (Exception e)
        {
            this.TryComplete(e);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ValidateState()
    {
        if (this.Completion.IsCompleted)
        {
            throw new InvalidOperationException("Cannot perform operation on closed channel");
        }
    }
}