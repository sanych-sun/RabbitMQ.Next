using System;
using System.Buffers;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Transport.Methods;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Transport.Channels
{
    public abstract class ContentMessageFrameHandlerBase<TMessage> : IFrameHandler
        where TMessage : struct, IIncomingMethod
    {
        private enum State
        {
            None = 0,
            ExpectHeader = 1,
            ExpectBody = 2,
        }

        private readonly IMethodParser<TMessage> methodParser;
        private readonly uint contentMethodId;
        private State state = State.None;
        private TMessage method;
        private IMessageProperties properties;


        public ContentMessageFrameHandlerBase(uint contentMethodId, IMethodParser<TMessage> methodParser)
        {
            this.methodParser = methodParser;
            this.contentMethodId = contentMethodId;
        }

        protected abstract void HandleMessage(TMessage method, IMessageProperties properties, ReadOnlySequence<byte> content);

        bool IFrameHandler.Handle(ChannelFrameType type, ReadOnlySequence<byte> payload)
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
            if (methodId != this.contentMethodId)
            {
                return false;
            }

            this.method = this.methodParser.Parse(payload);
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
                this.properties = props;
            }
            else
            {
                Span<byte> data = stackalloc byte[(int)payload.Length];
                payload.CopyTo(data);

                ((ReadOnlySpan<byte>) data).ReadMessageProperties(out var props);
                this.properties = props;
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

            this.HandleMessage(this.method, this.properties, payload);

            this.method = default;
            this.properties = null;

            this.state = State.None;
            return true;
        }
    }
}