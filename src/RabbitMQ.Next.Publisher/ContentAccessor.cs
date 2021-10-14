using System;
using System.Buffers;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Serialization;

namespace RabbitMQ.Next.Publisher
{
    internal sealed class ContentAccessor : IContentAccessor
    {
        private readonly ISerializerFactory serializerFactory;
        private ReadOnlySequence<byte> payload;
        private string payloadType;
        private bool isSet;

        public ContentAccessor(ISerializerFactory serializerFactory)
        {
            if (serializerFactory == null)
            {
                throw new ArgumentNullException(nameof(serializerFactory));
            }

            this.serializerFactory = serializerFactory;
        }

        public void Set(ReadOnlySequence<byte> content, string contentType)
        {
            this.payload = content;
            this.payloadType = contentType;
            this.isSet = true;
        }

        public void Reset()
        {
            this.payload = ReadOnlySequence<byte>.Empty;
            this.payloadType = string.Empty;
            this.isSet = false;
        }

        public T GetContent<T>()
        {
            if (!this.isSet)
            {
                throw new InvalidOperationException("Cannot get content outsize of message context");
            }

            var serializer = this.serializerFactory.Get(this.payloadType);

            if (serializer == null)
            {
                throw new NotSupportedException($"Cannot resolve a serializer for '{this.payloadType}' content type");
            }

            return serializer.Deserialize<T>(this.payload);
        }
    }
}