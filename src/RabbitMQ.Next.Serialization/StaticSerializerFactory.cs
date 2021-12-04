using System;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Serialization
{
    internal class StaticSerializerFactory : ISerializerFactory
    {
        private readonly ISerializer serializer;
        private readonly string contentType;

        public StaticSerializerFactory(ISerializer serializer, string contentType = null)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }

            this.serializer = serializer;
            this.contentType = contentType;
        }

        public ISerializer Get(IMessageProperties message)
        {
            if (string.IsNullOrEmpty(this.contentType) || string.Equals(this.contentType, message.ContentType))
            {
                return this.serializer;
            }

            throw new NotSupportedException($"Cannot resolve serializer for '{message.ContentType}' content type.");
        }
    }
}