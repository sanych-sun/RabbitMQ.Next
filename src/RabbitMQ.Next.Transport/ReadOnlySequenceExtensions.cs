using System;
using System.Buffers;

namespace RabbitMQ.Next.Transport
{
    public static class ReadOnlySequenceExtensions
    {
        public static ReadOnlySequence<byte> Read(this ReadOnlySequence<byte> payload, out uint methodId)
        {
            if (payload.FirstSpan.Length > sizeof(uint))
            {
                payload.FirstSpan.Read(out methodId);
            }
            else
            {
                Span<byte> buffer = stackalloc byte[sizeof(uint)];
                payload.Slice(0, sizeof(uint)).CopyTo(buffer);
                ((ReadOnlySpan<byte>)buffer).Read(out methodId);
            }

            return payload.Slice(sizeof(uint));
        }
    }
}