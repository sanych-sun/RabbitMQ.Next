using System;
using System.Buffers;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Serialization
{
    public class Serializer : ISerializer
    {
        private readonly IFormatterSource formatterSource;

        public Serializer(IFormatterSource formatterSource)
        {
            this.formatterSource = formatterSource;
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
            if (this.formatterSource.TryGetFormatter<TContent>(out var formatter))
            {
                return formatter;
            }

            throw new InvalidOperationException($"Cannot resolve formatter for the type: {typeof(TContent).FullName}");
        }
    }
}