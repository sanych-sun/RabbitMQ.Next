using System;
using System.Buffers;

namespace RabbitMQ.Next.Abstractions.Channels
{
    public interface IBufferWriter : IBufferWriter<byte>, IDisposable
    {
        ReadOnlySequence<byte> ToSequence();
    }
}