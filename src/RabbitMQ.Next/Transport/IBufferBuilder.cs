using System;

namespace RabbitMQ.Next.Transport;

/// <summary>
/// Interface almost identical to IBufferWriter, but assume usage of created buffers after calling Advance
/// Useful while writing dynamic structures into buffer (like a table or array)
/// </summary>
internal interface IBufferBuilder
{
    Span<byte> GetSpan(int sizeHint);
    
    Memory<byte> GetMemory(int sizeHint);

    void Advance(int count);
    
    int BytesWritten { get; }
}