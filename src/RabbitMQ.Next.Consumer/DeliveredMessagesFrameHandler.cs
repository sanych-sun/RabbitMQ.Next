using System;
using System.Buffers;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Channels;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Consumer
{
    internal class DeliveredMessagesFrameHandler : ContentMessageFrameHandlerBase<DeliverMethod>
    {
        private readonly Action<DeliverMethod, IMessageProperties, ReadOnlySequence<byte>> handler;

        public DeliveredMessagesFrameHandler(
            IMethodParser<DeliverMethod> contentMethodParser,
            Action<DeliverMethod, IMessageProperties, ReadOnlySequence<byte>> handler)
            : base((uint)MethodId.BasicDeliver, contentMethodParser)
        {
            this.handler = handler;
        }

        protected override void HandleMessage(DeliverMethod method, IMessageProperties properties, ReadOnlySequence<byte> content)
            => this.handler(method, properties, content);
    }
}