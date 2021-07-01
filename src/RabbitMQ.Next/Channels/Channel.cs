using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Buffers;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Exceptions;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Messaging;
using RabbitMQ.Next.Transport.Methods.Channel;

namespace RabbitMQ.Next.Channels
{
    internal sealed class Channel : IChannelInternal
    {
        private readonly SynchronizedChannel syncChannel;
        private readonly SemaphoreSlim senderSync;
        private readonly ChannelPool channelPool;
        private readonly IMethodRegistry registry;
        private readonly IBufferPool bufferPool;
        private readonly TaskCompletionSource<bool> channelCompletion;

        private readonly IReadOnlyList<IMethodHandler> methodHandlers;

        public Channel(ChannelPool channelPool, IMethodRegistry methodRegistry, IFrameSender frameSender, IBufferPool bufferPool, IReadOnlyList<IMethodHandler> handlers)
        {
            this.channelPool = channelPool;
            this.registry = methodRegistry;
            this.bufferPool = bufferPool;
            this.ChannelNumber = channelPool.Register(this);

            handlers ??= Array.Empty<IMethodHandler>();

            this.channelCompletion = new TaskCompletionSource<bool>();
            var pipe = new Pipe();
            this.Writer =  pipe.Writer;
            this.senderSync = new SemaphoreSlim(1,1);

            var waitFrameHandler = new WaitMethodHandler(methodRegistry, this);
            var list = new List<IMethodHandler>(handlers) { waitFrameHandler };
            if (this.ChannelNumber == 0)
            {
                list.Add(new ConnectionCloseHandler(this));
            }
            else
            {
                list.Add(new ChannelCloseHandler(this));
            }

            this.methodHandlers = list;
            this.syncChannel = new SynchronizedChannel(this.ChannelNumber, frameSender, waitFrameHandler);

            Task.Factory.StartNew(() => this.LoopAsync(pipe.Reader), TaskCreationOptions.LongRunning);
        }

        public ushort ChannelNumber { get; }

        public PipeWriter Writer { get; }

        public Task Completion => this.channelCompletion.Task;

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

            this.Writer.Complete();
            this.channelPool.Release(this.ChannelNumber);
        }

        public async ValueTask SendAsync<TRequest>(TRequest request, CancellationToken cancellation = default)
            where TRequest : struct, IOutgoingMethod
        {
            var isSync = this.registry.IsSync(request.MethodId);

            if (isSync)
            {
                await this.senderSync.WaitAsync(cancellation);
            }

            try
            {
                this.ValidateState();
                await this.syncChannel.SendAsync(request);
            }
            finally
            {
                if (isSync)
                {
                    this.senderSync.Release();
                }
            }
        }

        public async ValueTask UseChannel(Func<ISynchronizedChannel, ValueTask> fn, CancellationToken cancellation = default)
        {
            await this.senderSync.WaitAsync(cancellation);

            try
            {
                this.ValidateState();
                await fn(this.syncChannel);
            }
            finally
            {
                this.senderSync.Release();
            }
        }

        public async ValueTask UseChannel<TState>(TState state, Func<ISynchronizedChannel, TState, ValueTask> fn, CancellationToken cancellation = default)
        {
            await this.senderSync.WaitAsync(cancellation);

            try
            {
                this.ValidateState();
                await fn(this.syncChannel, state);
            }
            finally
            {
                this.senderSync.Release();
            }
        }

        public async ValueTask<TResult> UseChannel<TResult>(Func<ISynchronizedChannel, ValueTask<TResult>> fn, CancellationToken cancellation = default)
        {
            await this.senderSync.WaitAsync(cancellation);

            try
            {
                this.ValidateState();
                return await fn(this.syncChannel);
            }
            finally
            {
                this.senderSync.Release();
            }
        }

        public async ValueTask<TResult> UseChannel<TState, TResult>(TState state, Func<ISynchronizedChannel, TState, ValueTask<TResult>> fn, CancellationToken cancellation = default)
        {
            await this.senderSync.WaitAsync(cancellation);

            try
            {
                this.ValidateState();
                return await fn(this.syncChannel, state);
            }
            finally
            {
                this.senderSync.Release();
            }
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

        private async Task LoopAsync(PipeReader pipeReader)
        {
            Func<ReadOnlySequence<byte>, (ChannelFrameType Type, uint Size)> headerParser = ChannelFrame.ReadHeader;
            Func<ReadOnlySequence<byte>, IIncomingMethod> methodFrameParser = this.ParseMethodFrame;
            Func<IIncomingMethod, ReadOnlySequence<byte>, ValueTask<bool>> methodHandler = this.HandleMethodAsync;

            try
            {
                while (!this.Completion.IsCompleted)
                {
                    var header = await pipeReader.ReadAsync(ChannelFrame.FrameHeaderSize, headerParser);
                    if (header == default)
                    {
                        return;
                    }

                    var methodArgs = await pipeReader.ReadAsync(header.Size, methodFrameParser);

                    if (methodArgs == default)
                    {
                        return;
                    }

                    bool processed;
                    if (this.registry.HasContent(methodArgs.MethodId))
                    {
                        var contentHeader = await pipeReader.ReadAsync(ChannelFrame.FrameHeaderSize, headerParser);
                        if (header == default)
                        {
                            return;
                        }

                        processed = await pipeReader.ReadAsync(contentHeader.Size,
                            (methodArgs, methodHandler),
                            (state, sequence) => state.methodHandler(state.methodArgs, sequence));
                    }
                    else
                    {
                        processed = await methodHandler(methodArgs, default);
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

        private IIncomingMethod ParseMethodFrame(ReadOnlySequence<byte> payload)
        {
            payload = payload.Read(out uint methodId);
            var parser = this.registry.GetParser((MethodId)methodId);

            if (parser == null)
            {
                throw new NotSupportedException($"Cannot find parser for the method: {methodId}");
            }

            if (payload.IsSingleSegment)
            {
                return parser.ParseMethod(payload.FirstSpan);
            }

            using var buffer = this.bufferPool.CreateMemory((int)payload.Length);
            payload.CopyTo(buffer.Memory.Span);
            return parser.ParseMethod(buffer.Memory.Span);
        }

        private async ValueTask<bool> HandleMethodAsync(IIncomingMethod method, ReadOnlySequence<byte> content)
        {
            LazyMessageProperties properties = null;
            ReadOnlySequence<byte> contentBody = default;

            try
            {
                if (!content.IsEmpty)
                {
                    content = content.Read(out uint headerSize);
                    properties = new LazyMessageProperties(content.Slice(0, headerSize));
                    contentBody = content.Slice(headerSize);
                }

                for (var i = 0; i < this.methodHandlers.Count; i++)
                {
                    var handled = await this.methodHandlers[i].HandleAsync(method, properties, contentBody);
                    if (handled)
                    {
                        return true;
                    }
                }
            }
            finally
            {
                properties?.Dispose();
            }

            return false;
        }
    }
}