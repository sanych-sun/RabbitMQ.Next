using System;
using System.Buffers;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Transport;

namespace RabbitMQ.Next.Messaging.Common.Serializers
{
    public class IntSerializer : IMessageSerializer<int>
    {
        public void Serialize(IBufferWriter writer, int message)
        {
            var span = writer.GetSpan(sizeof(int));
            span.Write(message);
            writer.Advance(sizeof(int));
        }

        public int Deserialize(ReadOnlySequence<byte> bytes)
        {
            int result = 0;
            if (bytes.IsSingleSegment)
            {
                bytes.FirstSpan.Read(out result);
            }
            else
            {
                Span<byte> buffer = stackalloc byte[sizeof(int)];
                bytes.CopyTo(buffer);
                ((ReadOnlySpan<byte>) buffer).Read(out result);
            }

            return result;
        }
    }
}