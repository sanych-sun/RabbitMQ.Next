using System;
using System.Buffers;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Buffers;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Exceptions;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Transport.Channels
{
    public class ContentFrameHandler<TMessage> : IFrameHandler
        where TMessage : struct, IIncomingMethod
    {
        private readonly IBufferPool bufferPool;
        private readonly IMethodParser<TMessage> methodParser;
        private readonly uint contentMethodId;
        private readonly Action<TMessage, IMessageProperties, ReadOnlySequence<byte>> handler;
        private ContentFrameHandlerState state = ContentFrameHandlerState.None;
        private TMessage method;
        private IMessageProperties properties;


        public ContentFrameHandler(uint contentMethodId, IMethodParser<TMessage> methodParser, Action<TMessage, IMessageProperties, ReadOnlySequence<byte>> handler, IBufferPool bufferPool)
        {
            this.bufferPool = bufferPool;
            this.methodParser = methodParser;
            this.contentMethodId = contentMethodId;
            this.handler = handler;
        }

        public ContentFrameHandlerState ResetState()
        {
            var prevState = this.state;
            this.state = ContentFrameHandlerState.None;
            this.method = default;
            this.properties = null;

            return prevState;
        }

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
                    throw new ConnectionException(ReplyCode.UnexpectedFrame, $"Received unexpected {type} frame");
            }
        }

        private bool TryHandleMethodFrame(ReadOnlySequence<byte> payload)
        {
            if (this.state != ContentFrameHandlerState.None)
            {
                throw new ConnectionException(ReplyCode.UnexpectedFrame, $"Received unexpected method frame when in {this.state} state.");
            }

            payload = payload.Read(out uint methodId);
            if (methodId != this.contentMethodId)
            {
                return false;
            }

            if (payload.IsSingleSegment)
            {
                this.method = this.methodParser.Parse(payload.FirstSpan);
            }
            else
            {
                using var buffer = this.bufferPool.CreateMemory((int)payload.Length);
                payload.CopyTo(buffer.Memory.Span);

                this.method = this.methodParser.Parse(buffer.Memory.Span);
            }

            this.state = ContentFrameHandlerState.ExpectHeader;

            return true;
        }

        private bool TryHandleContentHeader(ReadOnlySequence<byte> payload)
        {
            if (this.state == ContentFrameHandlerState.None)
            {
                return false;
            }

            if (this.state != ContentFrameHandlerState.ExpectHeader)
            {
                throw new ConnectionException(ReplyCode.UnexpectedFrame, $"Received unexpected content header frame when in {this.state} state.");
            }

            if (payload.IsSingleSegment)
            {
                payload.FirstSpan.ReadMessageProperties(out var props);
                this.properties = props;
            }
            else
            {
                using var buffer = this.bufferPool.CreateMemory((int)payload.Length);
                payload.CopyTo(buffer.Memory.Span);

                ((ReadOnlySpan<byte>)buffer.Memory.Span).ReadMessageProperties(out var props);
                this.properties = props;
            }

            this.state = ContentFrameHandlerState.ExpectBody;
            return true;
        }

        private bool TryHandlerContentBody(ReadOnlySequence<byte> payload)
        {
            if (this.state == ContentFrameHandlerState.None)
            {
                return false;
            }

            if (this.state != ContentFrameHandlerState.ExpectBody)
            {
                throw new ConnectionException(ReplyCode.UnexpectedFrame, $"Received unexpected content frame when in {this.state} state.");
            }

            this.handler(this.method, this.properties, payload);

            this.ResetState();
            return true;
        }
    }
}