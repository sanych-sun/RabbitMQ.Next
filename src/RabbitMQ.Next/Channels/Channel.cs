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
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Buffers;
using RabbitMQ.Next.Tasks;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Messaging;
using RabbitMQ.Next.Transport.Methods.Channel;

namespace RabbitMQ.Next.Channels
{
    internal sealed class Channel : IChannelInternal
    {
        private readonly IMethodRegistry registry;
        private readonly ObjectPool<MemoryBlock> memoryPool;
        private readonly MethodSender methodSender;
        private readonly TaskCompletionSource<bool> channelCompletion;

        private readonly IList<IFrameHandler> frameHandlers;

        public Channel(IConnectionInternal connection, ushort channelNumber, int frameMaxSize)
        {
            this.ChannelNumber = channelNumber;
            this.registry = connection.MethodRegistry;
            this.memoryPool = connection.MemoryPool;

            this.methodSender = new MethodSender(this.ChannelNumber, connection.SocketWriter, connection.MethodRegistry, connection.FrameBuilderPool, frameMaxSize);

            this.channelCompletion = new TaskCompletionSource<bool>();
            var receiveChannel = System.Threading.Channels.Channel.CreateUnbounded<(FrameType Type, MemoryBlock Payload)>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = true,
                AllowSynchronousContinuations = false,
            });
            this.FrameWriter = receiveChannel.Writer;

            var channelCloseWait = new WaitMethodFrameHandler<CloseMethod>(this.registry);
            channelCloseWait.WaitTask.ContinueWith(t =>
            {
                if (t.IsCompleted)
                {
                    this.TryComplete(new ChannelException(t.Result.StatusCode, t.Result.Description, t.Result.FailedMethodId));
                }
            });

            this.frameHandlers = new List<IFrameHandler> { channelCloseWait };

            Task.Factory.StartNew(() => this.LoopAsync(receiveChannel.Reader), TaskCreationOptions.LongRunning);
        }

        public ushort ChannelNumber { get; }

        public void AddFrameHandler(IFrameHandler handler)
            => this.frameHandlers.Insert(0, handler);

        public bool RemoveFrameHandler(IFrameHandler handler)
            => this.frameHandlers.Remove(handler);

        public Task Completion => this.channelCompletion.Task;
        public ValueTask SendAsync<TRequest>(TRequest request, CancellationToken cancellation = default)
            where TRequest : struct, IOutgoingMethod
        {
            this.ValidateState();
            return this.methodSender.SendAsync(request, cancellation);
        }

        public async ValueTask<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellation = default)
            where TRequest : struct, IOutgoingMethod where TResponse : struct, IIncomingMethod
        {
            this.ValidateState();
            var waitTask = this.WaitAsync<TResponse>(cancellation);

            await this.methodSender.SendAsync(request, cancellation);

            return await waitTask;
        }

        public ValueTask PublishAsync<TState>(
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
            return true;
        }

        public async ValueTask<TMethod> WaitAsync<TMethod>(CancellationToken cancellation = default)
            where TMethod : struct, IIncomingMethod
        {
            this.ValidateState();
            var waitHandler = new WaitMethodFrameHandler<TMethod>(this.registry);
            this.AddFrameHandler(waitHandler);

            try
            {
                return await waitHandler.WaitTask.WithCancellation(cancellation);
            }
            finally
            {
                this.frameHandlers.Remove(waitHandler);
            }
        }

        public async ValueTask CloseAsync(Exception ex = null)
        {
            await this.SendAsync<CloseMethod, CloseOkMethod>(new CloseMethod((ushort) ReplyCode.Success, string.Empty, MethodId.Unknown));
            this.TryComplete(ex);
        }

        public async ValueTask CloseAsync(ushort statusCode, string description, MethodId failedMethodId)
        {
            await this.SendAsync<CloseMethod, CloseOkMethod>(new CloseMethod(statusCode, description, failedMethodId));
            this.TryComplete(new ChannelException(statusCode, description, failedMethodId));
        }

        private async Task LoopAsync(ChannelReader<(FrameType Type, MemoryBlock Payload)> reader)
        {
            var contentChunks = new List<MemoryBlock>();
            var messageProperty = new LazyMessageProperties();

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

                    // 2. Process method frame
                    var methodId = this.ProcessMethodFrame(methodFrame.Payload);

                    if (this.registry.HasContent(methodId))
                    {
                        var contentHeaderFrame = await reader.ReadAsync();
                        var payload = contentHeaderFrame.Payload.Memory[4..] // skip 2 obsolete shorts
                            .Read(out ulong contentSize);

                        try
                        {
                            messageProperty.Set(payload);

                            long receivedContent = 0;
                            while (receivedContent < (long)contentSize)
                            {
                                var frame = await reader.ReadAsync();
                                contentChunks.Add(frame.Payload);
                                receivedContent += frame.Payload.Memory.Length;
                            }

                            await this.ProcessContentAsync(messageProperty, contentChunks);
                        }
                        finally
                        {
                            this.memoryPool.Return(contentHeaderFrame.Payload);
                            messageProperty.Reset();
                            for (var i = 0; i < contentChunks.Count; i++)
                            {
                                this.memoryPool.Return(contentChunks[i]);
                            }
                            contentChunks.Clear();
                        }
                    }
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

        private MethodId ProcessMethodFrame(MemoryBlock payload)
        {
            try
            {
                var payloadBytes = payload.Memory.Read(out uint method);
                var methodId = (MethodId) method;
                for (var i = 0; i < this.frameHandlers.Count; i++)
                {
                    var handled = this.frameHandlers[i].HandleMethodFrame(methodId, payloadBytes);
                    if (handled)
                    {
                        break;
                    }
                }

                // todo: throw if not processed?

                return methodId;
            }
            finally
            {
                this.memoryPool.Return(payload);
            }
        }

        private async ValueTask ProcessContentAsync(LazyMessageProperties props, IReadOnlyList<MemoryBlock> contentFrames)
        {
            var content = contentFrames.ToSequence();

            for (var i = 0; i < this.frameHandlers.Count; i++)
            {
                var handled = await this.frameHandlers[i].HandleContentAsync(props, content);
                if (handled)
                {
                    return;
                }
            }
            // todo: throw if not processed?
        }
    }
}