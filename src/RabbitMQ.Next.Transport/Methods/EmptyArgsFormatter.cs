using System;

namespace RabbitMQ.Next.Transport.Methods
{
    internal class EmptyArgsFormatter<TMethod> : IMethodFormatter<TMethod>
        where TMethod: struct, IOutgoingMethod
    {
        public Span<byte> Write(Span<byte> destination, TMethod method)
            => destination;
    }
}