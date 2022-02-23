using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Serialization.PlainText
{
    internal class PlainTextSerializer : ISerializer
    {
        private readonly IConverter[] formatters;

        public PlainTextSerializer(IEnumerable<IConverter> formatters)
        {
            this.formatters = formatters?.ToArray();

            if (this.formatters == null || this.formatters.Length == 0)
            {
                throw new ArgumentNullException(nameof(formatters));
            }
        }

        public void Serialize<TContent>(TContent content, IBufferWriter<byte> writer)
        {
            for (var i = 0; i < this.formatters.Length; i++)
            {
                if (this.formatters[i].TryFormat(content, writer))
                {
                    return;
                }
            }

            throw new InvalidOperationException($"Cannot resolve formatter for the type: {typeof(TContent).FullName}");
        }

        public TContent Deserialize<TContent>(ReadOnlySequence<byte> bytes)
        {
            for (var i = 0; i < this.formatters.Length; i++)
            {
                if (this.formatters[i].TryParse(bytes, out TContent value))
                {
                    return value;
                }
            }

            throw new InvalidOperationException($"Cannot resolve formatter for the type: {typeof(TContent).FullName}");
        }
    }
}