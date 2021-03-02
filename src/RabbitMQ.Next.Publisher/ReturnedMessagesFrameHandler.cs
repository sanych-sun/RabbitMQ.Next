using System;
using System.Buffers;
using System.Collections.Generic;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Channels;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Publisher
{
    internal class ReturnedMessagesFrameHandler : ContentMessageFrameHandlerBase<ReturnMethod>
    {
        private readonly ContentAccessor contentAccessor;
        private readonly IReadOnlyList<Func<IReturnedMessage, IContent, bool>> handlers;

        public ReturnedMessagesFrameHandler(
            IMethodParser<ReturnMethod> returnContentMethodParser,
            ISerializer serializer,
            IReadOnlyList<Func<IReturnedMessage, IContent, bool>> handlers)
            : base((uint)MethodId.BasicReturn, returnContentMethodParser)
        {
            this.contentAccessor = new ContentAccessor(serializer);
            this.handlers = handlers;
        }

        protected override void HandleMessage(ReturnMethod method, IMessageProperties properties, ReadOnlySequence<byte> content)
        {
            this.contentAccessor.SetPayload(content);
            for (var i = 0; i < this.handlers.Count; i++)
            {
                var message = new ReturnedMessage
                {
                    Exchange = method.Exchange,
                    RoutingKey = method.RoutingKey,
                    ReplyCode = method.ReplyCode,
                    ReplyText = method.ReplyText,
                    Properties = properties
                };

                if (this.handlers[i].Invoke(message, this.contentAccessor))
                {
                    break;
                }
            }

            this.contentAccessor.SetPayload(ReadOnlySequence<byte>.Empty);
        }
    }
}