using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Exceptions;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Buffers;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Messaging;
using RabbitMQ.Next.Transport.Methods.Channel;

namespace RabbitMQ.Next.Channels
{
    internal sealed class Channel : IChannelInternal
    {
        private readonly ObjectPool<FrameBuilder> frameBuilderPool;
        private readonly ChannelWriter<IMemoryOwner<byte>> socketWriter;
        private readonly SemaphoreSlim senderSync;
        private readonly ChannelPool channelPool;
        private readonly IMethodRegistry registry;
        private readonly IBufferPool bufferPool;
        private readonly TaskCompletionSource<bool> channelCompletion;
        private readonly int frameMaxSize;

        private readonly WaitMethodHandler waitHandler;
        private readonly IReadOnlyList<IMethodHandler> methodHandlers;

        public Channel(ChannelPool channelPool, IMethodRegistry methodRegistry, ChannelWriter<IMemoryOwner<byte>> socketWriter, IBufferPool bufferPool, IReadOnlyList<IMethodHandler> handlers, int frameMaxSize)
        {
            this.channelPool = channelPool;
            this.registry = methodRegistry;
            this.socketWriter = socketWriter;
            this.bufferPool = bufferPool;
            this.ChannelNumber = channelPool.Register(this);
            this.frameMaxSize = frameMaxSize;

            this.frameBuilderPool = new DefaultObjectPool<FrameBuilder>(
                new ObjectPoolPolicy<FrameBuilder>(this.CreateFrameBuilder, ResetFrameBuilder), 100);

            handlers ??= Array.Empty<IMethodHandler>();

            this.channelCompletion = new TaskCompletionSource<bool>();
            var receiveChannel = System.Threading.Channels.Channel.CreateUnbounded<(FrameType Type, MemoryBlock Payload)>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = true,
                AllowSynchronousContinuations = false,
            });
            this.FrameWriter = receiveChannel.Writer;
            this.senderSync = new SemaphoreSlim(1,1);

            this.waitHandler = new WaitMethodHandler(methodRegistry, this);
            var list = new List<IMethodHandler>(handlers) { this.waitHandler };
            if (this.ChannelNumber == 0)
            {
                list.Add(new ConnectionCloseHandler(this));
            }
            else
            {
                list.Add(new ChannelCloseHandler(this));
            }

            this.methodHandlers = list;

            Task.Factory.StartNew(() => this.LoopAsync(receiveChannel.Reader), TaskCreationOptions.LongRunning);
        }

        public ushort ChannelNumber { get; }

        public Task Completion => this.channelCompletion.Task;

        public ChannelWriter<(FrameType Type, MemoryBlock Payload)> FrameWriter { get; }

        public void SetCompleted(Exception ex = null)
        {
            if (ex != null)
            {
                this.channelCompletion.SetException(ex);
            }
            else
            {
                this.channelCompletion.SetResult(true);
            }

            this.FrameWriter.Complete();
            this.channelPool.Release(this.ChannelNumber);
        }

        public async ValueTask<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : struct, IOutgoingMethod
            where TResponse : struct, IIncomingMethod
        {
            var waitTask = this.WaitAsync<TResponse>(cancellationToken);
            await this.SendAsync(request);
            return await waitTask;
        }

        public async ValueTask SendAsync<TRequest>(TRequest request, CancellationToken cancellation = default)
            where TRequest : struct, IOutgoingMethod
        {
            this.ValidateState();
            var frameBuilder = this.frameBuilderPool.Get();
            var buffer = frameBuilder.BeginMethodFrame(request.MethodId);

            var formatter = this.registry.GetFormatter<TRequest>();

            var bb = buffer.GetMemory();
            var written = bb.Length - formatter.Write(bb.Span, request).Length;
            buffer.Advance(written);
            frameBuilder.EndFrame();

            await this.senderSync.WaitAsync(cancellation);

            try
            {
                await frameBuilder.WriteToAsync(this.socketWriter);
            }
            finally
            {
                this.senderSync.Release();
                this.frameBuilderPool.Return(frameBuilder);
            }
        }

        public async ValueTask SendAsync<TState>(TState state, Action<TState, IFrameBuilder> payload)
        {
            this.ValidateState();
            var frameBuilder = this.frameBuilderPool.Get();
            payload(state, frameBuilder);

            await this.senderSync.WaitAsync();

            try
            {
                await frameBuilder.WriteToAsync(this.socketWriter);
            }
            finally
            {
                this.senderSync.Release();
                this.frameBuilderPool.Return(frameBuilder);
            }
        }

        public async ValueTask<TMethod> WaitAsync<TMethod>(CancellationToken cancellation = default)
            where TMethod : struct, IIncomingMethod
        {
            var result = await this.waitHandler.WaitAsync<TMethod>(cancellation);
            return (TMethod) result;
        }


        public async ValueTask CloseAsync(Exception ex = null)
        {
            await this.SendAsync<CloseMethod, CloseOkMethod>(new CloseMethod((ushort) ReplyCode.Success, string.Empty, MethodId.Unknown));
            this.SetCompleted(ex);
        }

        public async ValueTask CloseAsync(ushort statusCode, string description, MethodId failedMethodId)
        {
            await this.SendAsync<CloseMethod, CloseOkMethod>(new CloseMethod(statusCode, description, failedMethodId));
            this.SetCompleted(new ChannelException(statusCode, description, failedMethodId));
        }

        private async Task LoopAsync(ChannelReader<(FrameType Type, MemoryBlock Payload)> reader)
        {
            var contentChunks = new List<MemoryBlock>();

            try
            {
                while (!reader.Completion.IsCompleted)
                {
                    if (!reader.TryRead(out var methodFrame))
                    {
                        methodFrame = await reader.ReadAsync();
                    }

                    // 1. Expect method frame here
                    if (methodFrame.Type != FrameType.Method)
                    {
                        // TODO: connection exception?
                        throw new InvalidOperationException($"Unexpected frame type: {methodFrame.Type}");
                    }

                    // 2. Parse methdd
                    var method = this.ParseMethodFrame(methodFrame.Payload.Memory.Span);
                    methodFrame.Payload.Dispose();

                    bool processed;
                    if (this.registry.HasContent(method.MethodId))
                    {
                        var contentHeaderFrame = await reader.ReadAsync();
                        var contentHeader = this.ParseContentHeader(contentHeaderFrame.Payload.Memory);

                        long receivedContent = 0;
                        while (receivedContent < contentHeader.contentSize)
                        {
                            var frame = await reader.ReadAsync();
                            contentChunks.Add(frame.Payload);
                            receivedContent += frame.Payload.Memory.Length;
                        }

                        processed = await this.HandleMethodAsync(method, contentHeader.props, contentChunks.ToSequence());
                    }
                    else
                    {
                        processed = await this.HandleMethodAsync(method, null, default);
                    }

                    if (!processed)
                    {
                        // todo: close channel on unexpected method
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                this.SetCompleted(e);
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

        private IIncomingMethod ParseMethodFrame(ReadOnlySpan<byte> payload)
        {
            payload = payload.Read(out uint methodId);
            var parser = this.registry.GetParser((MethodId)methodId);

            if (parser == null)
            {
                throw new NotSupportedException($"Cannot find parser for the method: {methodId}");
            }

            return parser.ParseMethod(payload);
        }

        private (long contentSize, LazyMessageProperties props) ParseContentHeader(ReadOnlyMemory<byte> payload)
        {
            payload.Slice(4) // skip 2 obsolete shorts
                .Span.Read(out ulong contentSide);

            payload = payload.Slice(4 + sizeof(ulong));

            var props = new LazyMessageProperties(payload);
            return ((long)contentSide, props);
        }

        private async ValueTask<bool> HandleMethodAsync(IIncomingMethod method, LazyMessageProperties props, ReadOnlySequence<byte> content)
        {
            try
            {
                for (var i = 0; i < this.methodHandlers.Count; i++)
                {
                    var handled = await this.methodHandlers[i].HandleAsync(method, props, content);
                    if (handled)
                    {
                        return true;
                    }
                }
            }
            finally
            {
                props?.Dispose();
            }

            return false;
        }

        private FrameBuilder CreateFrameBuilder()
            => new FrameBuilder(this.bufferPool, this.ChannelNumber, this.frameMaxSize);

        private static bool ResetFrameBuilder(FrameBuilder fr)
        {
            fr.Reset();
            return true;
        }
    }
}