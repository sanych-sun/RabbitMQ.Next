using System;
using System.Buffers;

namespace RabbitMQ.Next.Serialization.Abstractions
{
    public interface IFormatter
    {
        bool CanHandle(Type type);

        void Format<TContent>(TContent content, IBufferWriter<byte> writer);

        TContent Parse<TContent>(ReadOnlySequence<byte> bytes);
    }
}