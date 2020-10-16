using System.Buffers;

namespace RabbitMQ.Next.Transport.Channels
{
    public interface IFrameHandler
    {
        bool Handle(FrameType type, ReadOnlySequence<byte> payload);
    }
}