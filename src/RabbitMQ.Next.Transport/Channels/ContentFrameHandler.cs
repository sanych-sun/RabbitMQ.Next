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
        private bool expectContent;
        private TMessage method;

        public ContentFrameHandler(uint contentMethodId, IMethodParser<TMessage> methodParser, Action<TMessage, IMessageProperties, ReadOnlySequence<byte>> handler, IBufferPool bufferPool)
        {
            this.bufferPool = bufferPool;
            this.methodParser = methodParser;
            this.contentMethodId = contentMethodId;
            this.handler = handler;
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
            this.expectContent = false;
        }

        private bool TryHandleMethodFrame(ReadOnlySequence<byte> payload)
        {
            if (this.expectContent)
            {
                throw new ConnectionException(ReplyCode.UnexpectedFrame, $"Received method frame when content frame was expected.");
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

            this.expectContent = true;

            return true;
        }

        private bool TryHandleContentFrame(ReadOnlySequence<byte> payload)
        {
            if (!this.expectContent)
            {
                return false;
            }

            payload = payload.Read(out int headerSize);
            var headerBytes = payload.Slice(0, headerSize);
            var contentBytes = payload.Slice(headerSize);

            var messageProps = new MessageProperties(headerBytes);
            this.handler(this.method, messageProps, contentBytes);

            // todo: dispose messageProps here to avoid buffers from leaking

            this.Reset();

            return true;
        }
    }
}