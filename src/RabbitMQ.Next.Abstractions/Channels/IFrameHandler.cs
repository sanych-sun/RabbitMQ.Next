using System;
using System.Buffers;
using System.Threading.Tasks;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Channels
{
    public interface IFrameHandler
    {
        bool HandleMethodFrame(MethodId methodId, ReadOnlySpan<byte> payload);

        ValueTask<bool> HandleContentAsync(IMessageProperties properties, ReadOnlySequence<byte> contentBytes);

        void Release(Exception ex = null);
    }
}