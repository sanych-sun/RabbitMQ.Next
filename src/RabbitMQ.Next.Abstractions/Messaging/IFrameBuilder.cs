using System.Buffers;
using System.Threading.Channels;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Buffers;

namespace RabbitMQ.Next.Abstractions.Messaging
{
    public interface IFrameBuilder
    {
        IBufferWriter<byte> BeginMethodFrame(MethodId methodId);

        IBufferWriter<byte> BeginContentFrame(MessageProperties properties);

        void EndFrame();

        ValueTask WriteToAsync(ChannelWriter<MemoryBlock> channel);
    }
}