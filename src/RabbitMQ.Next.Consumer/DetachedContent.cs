using System.Buffers;
using RabbitMQ.Next.Consumer.Abstractions;
using RabbitMQ.Next.Serialization;

namespace RabbitMQ.Next.Consumer
{
    internal class DetachedContent : IContent
    {
        private readonly ISerializer serializer;
        private readonly byte[] content;

        public DetachedContent(ISerializer serializer, ReadOnlySequence<byte> payload)
        {
            this.serializer = serializer;
            this.content = payload.ToArray();
        }

        public T GetContent<T>() => this.serializer.Deserialize<T>(new ReadOnlySequence<byte>(this.content));
    }
}