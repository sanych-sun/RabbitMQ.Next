using System;
using System.Buffers;
using RabbitMQ.Next.Consumer.Abstractions;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Consumer
{
    internal class ContentAccessor : IContent
    {
        private readonly ISerializer serializer;
        private ReadOnlySequence<byte> content;

        public ContentAccessor(ISerializer serializer)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }

            this.serializer = serializer;
        }

        internal void SetPayload(ReadOnlySequence<byte> payload)
        {
            this.content = payload;
        }

        public T GetContent<T>() => this.serializer.Deserialize<T>(this.content);

        public IContent AsDetached()
            => new DetachedContent(this.serializer, this.content);
    }
}