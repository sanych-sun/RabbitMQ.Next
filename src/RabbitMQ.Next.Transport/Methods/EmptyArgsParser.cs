using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods
{
    internal class EmptyArgsParser<TMethod> : IMethodParser<TMethod>
        where TMethod: struct, IIncomingMethod
    {
        public TMethod Parse(ReadOnlySpan<byte> payload)
            => default;

        public IIncomingMethod ParseMethod(ReadOnlySpan<byte> payload) => this.Parse(payload);
    }
}