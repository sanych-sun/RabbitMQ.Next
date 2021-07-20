using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;
using RabbitMQ.Next.Serialization.Abstractions;
using RabbitMQ.Next.Tasks;
using RabbitMQ.Next.Transport.Methods.Basic;
using RabbitMQ.Next.Transport.Methods.Channel;
using RabbitMQ.Next.Transport.Methods.Confirm;
using RabbitMQ.Next.Transport.Methods.Exchange;

namespace RabbitMQ.Next.Publisher
{
    internal sealed class Publisher : IPublisher
    {
        private readonly AsyncManualResetEvent flowControl = new AsyncManualResetEvent(true);
        private readonly SemaphoreSlim channelOpenSync = new SemaphoreSlim(1,1);
        private readonly IReadOnlyList<IReturnedMessageHandler> returnedMessageHandlers;
        private readonly IMethodHandler returnedFrameHandler;
        private readonly IConnection connection;
        private readonly string exchange;
        private readonly ISerializer serializer;
        private readonly IReadOnlyList<IMessageTransformer> transformers;
        private readonly IMethodFormatter<PublishMethod> publishFormatter;
        private readonly bool publisherConfirms;
        private readonly int maxFrameSize;
        private ConfirmMethodHandler confirms;
        private ulong lastDeliveryTag;
        private bool isDisposed;

        private IChannel channel;

        public Publisher(
            IConnection connection,
            string exchange,
            bool publisherConfirms,
            ISerializer serializer,
            IReadOnlyList<IMessageTransformer> transformers,
            IReadOnlyList<IReturnedMessageHandler> returnedMessageHandlers)
        {
            this.connection = connection;
            this.exchange = exchange;
            this.publisherConfirms = publisherConfirms;
            this.serializer = serializer;
            this.transformers = transformers;
            this.returnedMessageHandlers = returnedMessageHandlers;

            this.returnedFrameHandler = new MethodHandler<ReturnMethod>(this.HandleReturnedMessage);
            this.publishFormatter = connection.MethodRegistry.GetFormatter<PublishMethod>();
            this.maxFrameSize = connection.Details.FrameMaxSize;
        }

        public ValueTask CompleteAsync() => this.DisposeAsync();

        public ValueTask DisposeAsync()
        {
            if (this.isDisposed)
            {
                return default;
            }

            if (this.returnedMessageHandlers != null)
            {
                for (var i = 0; i < this.returnedMessageHandlers.Count; i++)
                {
                    this.returnedMessageHandlers[i].Dispose();
                }
            }

            this.isDisposed = true;
            var ch = this.channel;
            this.channel = null;

            if (ch != null)
            {
                return ch.CloseAsync();
            }

            return default;
        }


        public async ValueTask PublishAsync<TContent>(TContent content, string routingKey = null, MessageProperties properties = default, PublishFlags flags = PublishFlags.None, CancellationToken cancellationToken = default)
        {
            this.CheckDisposed();
            var ch = await this.EnsureChannelOpenAsync(cancellationToken);

            if (!this.flowControl.IsSet())
            {
                await this.flowControl.WaitAsync();
            }

            var message = this.ApplyTransformers(content, routingKey, properties, flags);

            this.CheckDisposed();

            await ch.SendAsync((message, publisher: this, content), (state, frameBuilder) =>
            {
                var method = new PublishMethod(state.publisher.exchange, state.message.RoutingKey, (byte) state.message.PublishFlags);
                var methodBuffer = frameBuilder.BeginMethodFrame(MethodId.BasicPublish);

                var bb = methodBuffer.GetSpan(state.publisher.maxFrameSize - 100);
                var written = bb.Length - state.publisher.publishFormatter.Write(bb, method).Length;
                methodBuffer.Advance(written);
                frameBuilder.EndFrame();

                var bodyWriter = frameBuilder.BeginContentFrame(state.message.Properties);
                state.publisher.serializer.Serialize(state.content, bodyWriter);
                frameBuilder.EndFrame();
            });

            var messageDeliveryTag = Interlocked.Increment(ref this.lastDeliveryTag);

            if (this.confirms != null)
            {
                var confirmed = await this.confirms.WaitForConfirmAsync(messageDeliveryTag, cancellationToken);
                if (!confirmed)
                {
                    // todo: provide some useful info here
                    throw new DeliveryFailedException();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ValueTask<IChannel> EnsureChannelOpenAsync(CancellationToken cancellationToken)
        {
            var ch = this.channel;

            if (ch == null)
            {
                return this.OpenChannelAsync(cancellationToken);
            }

            if (ch.Completion.Exception != null)
            {
                throw ch.Completion.Exception?.InnerException ?? ch.Completion.Exception;
            }

            return new ValueTask<IChannel>(ch);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (string RoutingKey, MessageProperties Properties, PublishFlags PublishFlags) ApplyTransformers<TContent>(
            TContent content, string routingKey, MessageProperties properties, PublishFlags publishFlags)
        {
            if (this.transformers == null)
            {
                return (routingKey, properties, publishFlags);
            }

            var messageBuilder = new MessageBuilder(routingKey, properties, publishFlags);
            for (var i = 0; i <= this.transformers.Count - 1; i++)
            {
                this.transformers[i].Apply(content, messageBuilder);
            }

            return (messageBuilder.RoutingKey, new MessageProperties(messageBuilder), messageBuilder.PublishFlags);
        }

        private async ValueTask<IChannel> OpenChannelAsync(CancellationToken cancellationToken)
        {
            // TODO: implement functionality to wait for Open state unless connection is closed
            if (this.connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException("Connection should be in Open state to use the API");
            }

            await this.channelOpenSync.WaitAsync(cancellationToken);

            try
            {
                if (this.channel == null || this.channel.Completion.IsCompleted)
                {
                    this.lastDeliveryTag = 0;
                    var methodHandlers = new List<IMethodHandler>();
                    methodHandlers.Add(this.returnedFrameHandler);
                    methodHandlers.Add(new MethodHandler<FlowMethod>((flow, _, _) =>
                    {
                        if (flow.Active)
                        {
                            this.flowControl.Set();
                        }
                        else
                        {
                            this.flowControl.Reset();
                        }

                        return new ValueTask<bool>(true);
                    }));

                    if (this.publisherConfirms)
                    {
                        this.confirms = new ConfirmMethodHandler();
                        methodHandlers.Add(this.confirms);
                    }

                    this.channel = await this.connection.OpenChannelAsync(methodHandlers, cancellationToken);
                    await this.channel.UseChannel(
                        (this.exchange, this.publisherConfirms),
                        async (ch, state) =>
                        {
                            await ch.SendAsync<DeclareMethod, DeclareOkMethod>(new DeclareMethod(state.exchange));
                            if (state.publisherConfirms)
                            {
                                await ch.SendAsync<SelectMethod, SelectOkMethod>(new SelectMethod(false));
                            }
                        }, cancellationToken);
                }

                return this.channel;
            }
            finally
            {
                this.channelOpenSync.Release();
            }
        }

        private async ValueTask<bool> HandleReturnedMessage(ReturnMethod method, IMessageProperties properties, ReadOnlySequence<byte> contentBytes)
        {
            if (this.returnedMessageHandlers.Count == 0)
            {
                return true;
            }

            var message = new ReturnedMessage(method.Exchange, method.RoutingKey, method.ReplyCode, method.ReplyText);
            var content = new Content(this.serializer, contentBytes);

            for (var i = 0; i < this.returnedMessageHandlers.Count; i++)
            {
                if (await this.returnedMessageHandlers[i].TryHandleAsync(message, properties, content))
                {
                    break;
                }
            }

            return true;
        }
    }
}