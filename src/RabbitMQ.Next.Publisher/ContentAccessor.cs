using System;
using System.Buffers;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Publisher
{
    internal sealed class ContentAccessor : IContentAccessor
    {
        private readonly ISerializer serializer;
        private ReadOnlySequence<byte> payload;

        public ContentAccessor(ISerializer serializer)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }

            this.serializer = serializer;
        }

        public void Set(ReadOnlySequence<byte> content)
        {
            this.payload = content;
        }

        public void Reset()
        {
            this.payload = ReadOnlySequence<byte>.Empty;
        }

        public T GetContent<T>() => this.serializer.Deserialize<T>(this.payload);
    }
}