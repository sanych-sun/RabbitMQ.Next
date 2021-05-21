using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Transport.Methods.Channel;

namespace RabbitMQ.Next.Transport.Channels
{
    internal sealed class Channel : IChannelInternal
    {
        private readonly SynchronizedChannel syncChannel;
        private readonly SemaphoreSlim senderSync;
        private readonly ChannelPool channelPool;
        private readonly TaskCompletionSource<bool> channelCompletion;

        private readonly IReadOnlyList<IFrameHandler> frameHandlers;

        public Channel(ChannelPool channelPool, IMethodRegistry methodRegistry, IFrameSender frameSender, IEnumerable<IFrameHandler> handlers)
        {
            this.channelPool = channelPool;

            handlers ??= Array.Empty<IFrameHandler>();

            this.channelCompletion = new TaskCompletionSource<bool>();
            var pipe = new Pipe();
            this.Writer =  new ChannelWriter(pipe.Writer);
            this.senderSync = new SemaphoreSlim(1,1);

            var waitFrameHandler = new WaitMethodFrameHandler(methodRegistry, this);
            var channelCloseHandler = new ChannelCloseHandler(methodRegistry, this);
            this.frameHandlers = new List<IFrameHandler>(handlers) { waitFrameHandler, channelCloseHandler };
            this.ChannelNumber = channelPool.Register(this);
            this.syncChannel = new SynchronizedChannel(this.ChannelNumber, frameSender, waitFrameHandler);

            Task.Run(() => this.LoopAsync(pipe.Reader));
        }

        public ushort ChannelNumber { get; }

        public ChannelWriter Writer { get; }

        public Task Completion => this.channelCompletion.Task;

        public void SetCompleted(Exception ex = null)
        {
            this.Writer.Dispose();
            if (ex != null)
            {
                this.channelCompletion.SetException(ex);
            }
            else
            {
                this.channelCompletion.SetResult(true);
            }
        }

        public async Task SendAsync<TMethod>(TMethod request)
            where TMethod : struct, IOutgoingMethod
        {
            await this.senderSync.WaitAsync();

            try
            {
                this.ValidateState();
                await this.syncChannel.SendAsync(request);
            }
            finally
            {
                this.senderSync.Release();
            }
        }

        public async Task SendAsync<TMethod>(TMethod request, IMessageProperties properties, ReadOnlySequence<byte> content)
            where TMethod : struct, IOutgoingMethod
        {
            await this.senderSync.WaitAsync();

            try
            {
                this.ValidateState();
                await this.syncChannel.SendAsync(request, properties, content);
            }
            finally
            {
                this.senderSync.Release();
            }
        }

        public async Task<TMethod> WaitAsync<TMethod>(CancellationToken cancellation = default)
            where TMethod : struct, IIncomingMethod
        {
            await this.senderSync.WaitAsync(cancellation);

            try
            {
                this.ValidateState();
                return await this.syncChannel.WaitAsync<TMethod>(cancellation);
            }
            finally
            {
                this.senderSync.Release();
            }
        }

        public async Task UseSyncChannel<TState>(TState state, Func<ISynchronizedChannel, TState, Task> fn)
        {
            await this.senderSync.WaitAsync();

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

        public async Task<TResult> UseSyncChannel<TResult, TState>(TState state, Func<ISynchronizedChannel, TState, Task<TResult>> fn)
        {
            await this.senderSync.WaitAsync();

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

        public Task CloseAsync()
            => this.CloseAsync((ushort)ReplyCode.Success, string.Empty, 0);

        public async Task CloseAsync(ushort statusCode, string description, uint failedMethodId)
        {
            await this.SendAsync<CloseMethod, CloseOkMethod>(new CloseMethod(statusCode, description, failedMethodId));

            this.channelPool.Release(this.ChannelNumber);
        }

        private async Task LoopAsync(PipeReader pipeReader)
        {
            Func<ReadOnlySequence<byte>, (FrameType Type, int Size)> headerParser = ChannelFrame.ReadChannelHeader;
            Func<ReadOnlySequence<byte>, bool> methodFrameHandler = (sequence) => this.ParseFramePayload(FrameType.Method, sequence);
            Func<ReadOnlySequence<byte>, bool> contentHeaderFrameHandler = (sequence) => this.ParseFramePayload(FrameType.ContentHeader, sequence);
            Func<ReadOnlySequence<byte>, bool> contentBodyFrameHandler = (sequence) => this.ParseFramePayload(FrameType.ContentBody, sequence);

            while (!this.Completion.IsCompleted)
            {
                var header = await pipeReader.ReadAsync(ChannelFrame.FrameHeaderSize, headerParser);
                if (header == default)
                {
                    return;
                }

                Func<ReadOnlySequence<byte>, bool> payloadHandler;
                switch (header.Type)
                {
                    case FrameType.Method:
                        payloadHandler = methodFrameHandler;
                        break;
                    case FrameType.ContentHeader:
                        payloadHandler = contentHeaderFrameHandler;
                        break;
                    case FrameType.ContentBody:
                        payloadHandler = contentBodyFrameHandler;
                        break;
                    default:
                        // todo: throw connection exception here for unexpected frame
                        throw new InvalidOperationException();
                }

                var processed = await pipeReader.ReadAsync(header.Size, payloadHandler);

                if (!processed)
                {
                    return;
                }
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

        private bool ParseFramePayload(FrameType type, ReadOnlySequence<byte> sequence)
        {
            for (var i = 0; i < this.frameHandlers.Count; i++)
            {
                if (this.frameHandlers[i].Handle((ChannelFrameType)type, sequence))
                {
                    return true;
                }
            }

            return false;
        }
    }
}