using System;
using System.Buffers;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Abstractions.Channels
{
    public interface IFrameHandler
    {
        ValueTask<bool> HandleMethodFrameAsync(MethodId methodId, ReadOnlyMemory<byte> payload);

        ValueTask<bool> HandleContentAsync(IMessageProperties properties, ReadOnlySequence<byte> contentBytes);

        void Reset();
    }
}