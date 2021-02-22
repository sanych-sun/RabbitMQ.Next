using System;
using System.Buffers;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Serialization.Abstractions;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.MessagePublisher
{
    internal class ReturnedMessagesFrameHandler : IFrameHandler
    {
        private enum State
        {
            None = 0,
            ExpectHeader = 1,
            ExpectBody = 2,
        }

        private readonly IMethodParser<ReturnMethod> returnMethodParser;
        private readonly ISerializer serializer;
        private readonly ReturnedMessageDelegateCollection handlers;
        private State state = State.None;
        private ReturnedMessage message;


        public ReturnedMessagesFrameHandler(IMethodParser<ReturnMethod> returnMethodParser, ISerializer serializer, ReturnedMessageDelegateCollection handlers)
        {
            this.returnMethodParser = returnMethodParser;
            this.serializer = serializer;
            this.handlers = handlers;
        }

        public bool Handle(ChannelFrameType type, ReadOnlySequence<byte> payload)
        {
            switch (type)
            {
                case ChannelFrameType.Method:
                    return this.TryHandleMethodFrame(payload);
                case ChannelFrameType.ContentHeader:
                    return this.TryHandleContentHeader(payload);
                case ChannelFrameType.ContentBody:
                    return this.TryHandlerContentBody(payload);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private bool TryHandleMethodFrame(ReadOnlySequence<byte> payload)
        {
            payload = payload.Read(out uint methodId);
            if (methodId != (uint) MethodId.BasicReturn)
            {
                return false;
            }

            var args = this.returnMethodParser.Parse(payload);
            this.message = new ReturnedMessage
            {
                Exchange = args.Exchange,
                RoutingKey = args.RoutingKey,
                ReplyCode = args.ReplyCode,
                ReplyText = args.ReplyText,
            };
            this.state = State.ExpectHeader;

            return true;
        }

        private bool TryHandleContentHeader(ReadOnlySequence<byte> payload)
        {
            if (this.state != State.ExpectHeader)
            {
                return false;
            }

            if (payload.IsSingleSegment)
            {
                payload.FirstSpan.ReadMessageProperties(out var props);
                this.message.Properties = props;
            }
            else
            {
                Span<byte> data = stackalloc byte[(int)payload.Length];
                payload.CopyTo(data);

                ((ReadOnlySpan<byte>) data).ReadMessageProperties(out var props);
                this.message.Properties = props;
            }

            this.state = State.ExpectBody;
            return true;
        }

        private bool TryHandlerContentBody(ReadOnlySequence<byte> payload)
        {
            if (this.state != State.ExpectBody)
            {
                return false;
            }

            this.handlers.Invoke(this.message, new Content(this.serializer, payload));

            this.state = State.None;
            return true;
        }
    }
}