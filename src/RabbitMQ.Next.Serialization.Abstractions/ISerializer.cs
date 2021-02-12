using System.Buffers;

namespace RabbitMQ.Next.Serialization.Abstractions
{
    public interface ISerializer
    {
        void Serialize<TContent>(TContent content, IBufferWriter<byte> writer);

        TContent Deserialize<TContent>(ReadOnlySequence<byte> bytes);
    }
}