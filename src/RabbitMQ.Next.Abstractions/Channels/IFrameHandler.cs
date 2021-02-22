using System.Buffers;

namespace RabbitMQ.Next.Abstractions.Channels
{
    public interface IFrameHandler
    {
        bool Handle(ChannelFrameType type, ReadOnlySequence<byte> payload);
    }
}