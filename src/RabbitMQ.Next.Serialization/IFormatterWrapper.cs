using System.Buffers;
using RabbitMQ.Next.Abstractions;

namespace RabbitMQ.Next.Serialization
{
    internal interface IFormatterWrapper
    {
        void Format(object content, IBufferWriter writer);

        object Parse(ReadOnlySequence<byte> bytes);
    }
}