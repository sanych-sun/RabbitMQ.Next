using System;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Tests.Mocks
{
    public class DummyFormatter<TMessage> : IMethodFormatter<TMessage>
        where TMessage: struct, IOutgoingMethod
    {
        private readonly byte[] payload;

        public DummyFormatter(byte[] payload)
        {
            this.payload = payload;
        }

        public int Write(Span<byte> destination, TMessage method)
        {
            this.payload.CopyTo(destination);
            return this.payload.Length;
        }
    }
}