using System;
using System.IO;

namespace RabbitMQ.Next.Buffers;

internal interface IMemoryAccessor : IDisposable
{
    int Size { get; }
    
    ReadOnlyMemory<byte> Memory { get; }
    
    IMemoryAccessor Next { get; }

    IMemoryAccessor Append(IMemoryAccessor next);

    void WriteTo(Stream stream);
}