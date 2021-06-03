using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Confirm
{
    internal class SelectMethodFormatter : IMethodFormatter<SelectMethod>
    {
        public Span<byte> Write(Span<byte> destination, SelectMethod method)
            => destination.Write(method.NoWait);
    }
}