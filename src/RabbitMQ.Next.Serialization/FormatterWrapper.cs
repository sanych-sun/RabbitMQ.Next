using System.Buffers;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Serialization.Abstractions;

namespace RabbitMQ.Next.Serialization
{
    internal class FormatterWrapper<TContent> : IFormatterWrapper
    {
        private readonly IFormatter<TContent> wrapped;

        public FormatterWrapper(IFormatter<TContent> formatter)
        {
            this.wrapped = formatter;
        }

        public void Format(object content, IBufferWriter writer)
            => this.wrapped.Format((TContent)content, writer);

        public object Parse(ReadOnlySequence<byte> bytes)
            => this.wrapped.Parse(bytes);
    }
}