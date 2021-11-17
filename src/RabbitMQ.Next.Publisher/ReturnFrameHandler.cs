using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Publisher
{
    internal sealed class ReturnFrameHandler : IFrameHandler
    {
        private readonly IReadOnlyList<IReturnedMessageHandler> messageHandlers;
        private readonly IMethodParser<ReturnMethod> returnMethodParser;
        private readonly ContentAccessor contentAccessor;

        private bool expectContent;
        private ReturnMethod currentMethod;

        public ReturnFrameHandler(ISerializerFactory serializerFactory, IReadOnlyList<IReturnedMessageHandler> messageHandlers, IMethodParser<ReturnMethod> returnMethodParser)
        {
            this.messageHandlers = messageHandlers;
            this.returnMethodParser = returnMethodParser;
            this.contentAccessor = new ContentAccessor(serializerFactory);
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

        public void Reset()
        {
            this.expectContent = false;
            this.currentMethod = default;
            this.contentAccessor.Reset();
        }

        private async ValueTask<bool> HandleReturnedMessageAsync(IMessageProperties properties, ReadOnlySequence<byte> contentBytes)
        {
            var message = new ReturnedMessage(this.currentMethod.Exchange, this.currentMethod.RoutingKey, this.currentMethod.ReplyCode, this.currentMethod.ReplyText);
            this.contentAccessor.Set(contentBytes, properties.ContentType);

            try
            {
                for (var i = 0; i < this.messageHandlers.Count; i++)
                {
                    if (await this.messageHandlers[i].TryHandleAsync(message, properties, this.contentAccessor))
                    {
                        break;
                    }
                }
            }
            finally
            {
                this.Reset();
            }

            return true;
        }
    }
}