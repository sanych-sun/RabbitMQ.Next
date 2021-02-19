using System;
using System.Buffers;

namespace RabbitMQ.Next.Abstractions.Buffers
{
    public interface IBufferWriter : IBufferWriter<byte>, IDisposable
    {
        ReadOnlySequence<byte> ToSequence();
    }
}