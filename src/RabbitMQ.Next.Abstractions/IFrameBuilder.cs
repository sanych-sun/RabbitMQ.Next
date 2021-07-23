using System.Buffers;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Abstractions
{
    public interface IFrameBuilder
    {
        IBufferWriter<byte> BeginMethodFrame(MethodId methodId);

        IBufferWriter<byte> BeginContentFrame(IMessageProperties properties);

        void EndFrame();
    }
}