using System;
using System.Buffers;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Serialization
{
    public class DynamicSerializer : ISerializer
    {
        private readonly FormatterRegistry formatters;

        public DynamicSerializer(FormatterRegistry registry)
        {
            this.formatters = registry;
        }

        public void Serialize<TContent>(TContent content, IBufferWriter writer)
            => this.GetFormatter<TContent>().Format(content, writer);

        public TContent Deserialize<TContent>(ReadOnlySequence<byte> bytes)
        {
            var formatter = this.GetFormatter<TContent>();
            return (TContent) formatter.Parse(bytes);
        }

        private IFormatterWrapper GetFormatter<TContent>()
        {
            var formatter = this.formatters.GetFormatter<TContent>();
            if (formatter == null)
            {
                throw new NotSupportedException($"Cannot resolve formatter for the content: {typeof(TContent).FullName}");
            }

            return formatter;
        }
    }
}