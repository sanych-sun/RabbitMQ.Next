using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Serialization.PlainText
{
    internal class PlainTextSerializer : ISerializer
    {
        public PlainTextSerializer(IEnumerable<IFormatter> formatters)
        {
            this.Formatters = formatters?.ToArray();

            if (this.Formatters == null || this.Formatters.Count == 0)
            {
                throw new ArgumentNullException(nameof(formatters));
            }
        }

        internal IReadOnlyList<IFormatter> Formatters { get; }

        public void Serialize<TContent>(TContent content, IBufferWriter<byte> writer)
            => this.GetFormatter<TContent>().Format(content, writer);

        public TContent Deserialize<TContent>(ReadOnlySequence<byte> bytes)
        {
            var formatter = this.GetFormatter<TContent>();
            return formatter.Parse<TContent>(bytes);
        }

        private IFormatter GetFormatter<TContent>()
        {
            for (var i = 0; i < this.Formatters.Count; i++)
            {
                if (this.Formatters[i].CanHandle(typeof(TContent)))
                {
                    return this.Formatters[i];
                }
            }

            throw new InvalidOperationException($"Cannot resolve formatter for the type: {typeof(TContent).FullName}");
        }
    }
}