using System;
using System.Buffers;

namespace RabbitMQ.Next.Transport
{
    public static class ReadOnlySequenceExtensions
    {
        public static ReadOnlySequence<byte> Read(this ReadOnlySequence<byte> payload, out uint data)
        {
            if (payload.FirstSpan.Length > sizeof(uint))
            {
                payload.FirstSpan.Read(out data);
            }
            else
            {
                Span<byte> buffer = stackalloc byte[sizeof(uint)];
                payload.Slice(0, sizeof(uint)).CopyTo(buffer);
                ((ReadOnlySpan<byte>)buffer).Read(out data);
            }

            return payload.Slice(sizeof(uint));
        }

        public static ReadOnlySequence<byte> Read(this ReadOnlySequence<byte> payload, out int data)
        {
            if (payload.FirstSpan.Length > sizeof(int))
            {
                payload.FirstSpan.Read(out data);
            }
            else
            {
                Span<byte> buffer = stackalloc byte[sizeof(int)];
                payload.Slice(0, sizeof(int)).CopyTo(buffer);
                ((ReadOnlySpan<byte>)buffer).Read(out data);
            }

            return payload.Slice(sizeof(int));
        }
    }
}