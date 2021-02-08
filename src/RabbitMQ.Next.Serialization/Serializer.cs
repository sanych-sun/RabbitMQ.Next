using System;
using System.Buffers;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Serialization
{
    public class Serializer<TContent> : ISerializer
    {
        private readonly IFormatter<TContent> formatter;

        public Serializer(IFormatter<TContent> formatter)
        {
            this.formatter = formatter;
        }

        public void Serialize<TContent1>(TContent1 content, IBufferWriter writer)
        {
            if (content is TContent c)
            {
                this.formatter.Format(c, writer);
                return;
            }

            throw new NotSupportedException();
        }

        public TContent1 Deserialize<TContent1>(ReadOnlySequence<byte> bytes)
        {
            if (typeof(TContent).IsAssignableFrom(typeof(TContent1)))
            {
                var c = this.formatter.Parse(bytes);
                if(c is TContent1 result)
                {
                    return result;
                }
            }

            throw new NotSupportedException();
        }
    }
}