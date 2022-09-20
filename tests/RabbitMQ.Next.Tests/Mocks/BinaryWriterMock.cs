using System;
using RabbitMQ.Next.Transport;

namespace RabbitMQ.Next.Tests.Mocks;

internal class BinaryWriterMock : IBinaryWriter
{
    private readonly byte[] buffer;

    public BinaryWriterMock(int size)
    {
        this.buffer = new byte[size];
    }
    
    private int bytesWritten;

    public Span<byte> GetSpan(int sizeHint) 
        =>  new Span<byte>(this.buffer).Slice(this.bytesWritten);

    public Memory<byte> GetMemory(int sizeHint)
        =>  new Memory<byte>(this.buffer).Slice(this.bytesWritten);

    public void Advance(int count)
    {
        this.bytesWritten += count;
    }

    public int BytesWritten => this.bytesWritten;

    public Span<byte> Written => new Span<byte>(this.buffer, 0, this.bytesWritten);
}