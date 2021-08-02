using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Serialization.Abstractions;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Publisher
{
    internal sealed class ReturnFrameHandler : IFrameHandler
    {
        private readonly ISerializer serializer;
        private readonly IReadOnlyList<IReturnedMessageHandler> returnedMessageHandlers;
        private readonly IMethodParser<ReturnMethod> returnMethodParser;

        private bool expectContent;
        private ReturnMethod currentMethod;

        public ReturnFrameHandler(ISerializer serializer, IReadOnlyList<IReturnedMessageHandler> returnedMessageHandlers, IMethodRegistry registry)
        {
            this.serializer = serializer;
            this.returnedMessageHandlers = returnedMessageHandlers;
            this.returnMethodParser = registry.GetParser<ReturnMethod>();
        }

        public ValueTask<bool> HandleMethodFrameAsync(MethodId methodId, ReadOnlyMemory<byte> payload)
        {
            if (methodId != MethodId.BasicReturn)
            {
                return new(false);
            }

            this.currentMethod = this.returnMethodParser.Parse(payload);
            this.expectContent = true;
            return new(true);
        }

        public ValueTask<bool> HandleContentAsync(IMessageProperties properties, ReadOnlySequence<byte> contentBytes)
        {
            if (!this.expectContent)
            {
                return new(false);
            }

            return this.HandleReturnedMessageAsync(properties, contentBytes);
        }

        private async ValueTask<bool> HandleReturnedMessageAsync(IMessageProperties properties, ReadOnlySequence<byte> contentBytes)
        {
            var message = new ReturnedMessage(this.currentMethod.Exchange, this.currentMethod.RoutingKey, this.currentMethod.ReplyCode, this.currentMethod.ReplyText);
            var content = new Content(this.serializer, contentBytes);

            try
            {
                for (var i = 0; i < this.returnedMessageHandlers.Count; i++)
                {
                    if (await this.returnedMessageHandlers[i].TryHandleAsync(message, properties, content))
                    {
                        break;
                    }
                }

                // todo: throw if not handled?
            }
            finally
            {
                this.expectContent = false;
                this.currentMethod = default;
                content.Dispose();
            }

            return true;
        }
    }
}