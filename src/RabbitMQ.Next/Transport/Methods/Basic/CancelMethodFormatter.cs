using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic
{
    internal class CancelMethodFormatter : IMethodFormatter<CancelMethod>
    {
        public int Write(Span<byte> destination, CancelMethod method)
        {
            var result = destination
                .Write(method.ConsumerTag)
                .Write(false); // noWait flag

            return destination.Length - result.Length;
        }
    }
}