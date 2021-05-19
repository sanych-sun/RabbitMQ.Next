using System;
using System.Buffers;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Buffers;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Exceptions;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Transport.Messaging;

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

            return prevState;
        }

        bool IFrameHandler.Handle(ChannelFrameType type, ReadOnlySequence<byte> payload)
        {
            switch (type)
            {
                case ChannelFrameType.Method:
                    return this.TryHandleMethodFrame(payload);
                case ChannelFrameType.Content:
                    return this.TryHandleContentFrame(payload);
                default:
                    throw new ConnectionException(ReplyCode.UnexpectedFrame, $"Received unexpected {type} frame");
            }
        }

        public void Reset()
        {
            this.method = default;
            this.state = ContentFrameHandlerState.None;
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

            this.state = ContentFrameHandlerState.ExpectContent;

            return true;
        }

        private bool TryHandleContentFrame(ReadOnlySequence<byte> payload)
        {
            if (this.state == ContentFrameHandlerState.None)
            {
                return false;
            }

            payload = payload.Read(out int headerSize);
            var headerBytes = payload.Slice(0, headerSize);
            var contentBytes = payload.Slice(headerSize);

            var messageProps = new MessageProperties(headerBytes);
            this.handler(this.method, messageProps, contentBytes);

            // todo: dispose messageProps here to avoid buffers from leaking

            this.ResetState();

            return true;
        }
    }
}