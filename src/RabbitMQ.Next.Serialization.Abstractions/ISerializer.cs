using System.Buffers;
using RabbitMQ.Next.Abstractions;

namespace RabbitMQ.Next.Serialization.Abstractions
{
    public interface ISerializer
    {
        void Serialize<TContent>(TContent content, IBufferWriter writer);

        TContent Deserialize<TContent>(ReadOnlySequence<byte> bytes);
    }
}