using System.Buffers;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Publisher
{
    internal class Content : IContent
    {
        private readonly ISerializer serializer;
        private readonly ReadOnlySequence<byte> payload;

        public Content(ISerializer serializer, ReadOnlySequence<byte> payload)
        {
            this.serializer = serializer;
            this.payload = payload;
        }

        public T GetContent<T>() => this.serializer.Deserialize<T>(this.payload);
    }
}