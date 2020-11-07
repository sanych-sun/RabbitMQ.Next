using System;
using System.Buffers;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Messaging.Common.Serializers
{
    public class ArraySerializer : IMessageSerializer<byte[]>
    {
        public void Serialize(IBufferWriter writer, byte[] message)
        {
            ReadOnlySpan<byte> source = message;

            do
            {
                var target = writer.GetSpan();
                var chunk = source;
                if (source.Length > target.Length)
                {
                    chunk = source.Slice(0, target.Length);
                }

                chunk.CopyTo(target);
                writer.Advance(chunk.Length);
                source = source.Slice(chunk.Length);

            } while (source.Length > 0);
        }

        public byte[] Deserialize(ReadOnlySequence<byte> bytes) => bytes.ToArray();
    }
}