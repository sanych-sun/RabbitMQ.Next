using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Serialization.PlainText
{
    internal class PlainTextSerializer : ISerializer
    {
        private readonly IFormatter[] formatters;

        public PlainTextSerializer(IEnumerable<IFormatter> formatters)
        {
            this.formatters = formatters?.ToArray();

            if (this.formatters == null || this.formatters.Length == 0)
            {
                throw new ArgumentNullException(nameof(formatters));
            }
        }

        public void Serialize<TContent>(TContent content, IBufferWriter<byte> writer)
            => this.GetFormatter<TContent>().Format(content, writer);

        public TContent Deserialize<TContent>(ReadOnlySequence<byte> bytes)
        {
            var formatter = this.GetFormatter<TContent>();
            return formatter.Parse<TContent>(bytes);
        }

        private IFormatter GetFormatter<TContent>()
        {
            for (var i = 0; i < this.formatters.Length; i++)
            {
                if (this.formatters[i].CanHandle(typeof(TContent)))
                {
                    return this.formatters[i];
                }
            }

            throw new InvalidOperationException($"Cannot resolve formatter for the type: {typeof(TContent).FullName}");
        }
    }
}