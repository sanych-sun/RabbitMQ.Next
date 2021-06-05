using System;
using System.Buffers;
using RabbitMQ.Next.Transport;

namespace RabbitMQ.Next.Channels
{
    internal static class ReadOnlySequenceExtensions
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
    }
}