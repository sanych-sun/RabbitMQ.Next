using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods
{
    internal class EmptyArgsFormatter<TMethod> : IMethodFormatter<TMethod>
        where TMethod: struct, IOutgoingMethod
    {
        public int Write(Memory<byte> destination, TMethod method) => 0;
    }
}