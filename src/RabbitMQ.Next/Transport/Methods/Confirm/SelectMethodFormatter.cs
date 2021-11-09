using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Confirm
{
    internal class SelectMethodFormatter : IMethodFormatter<SelectMethod>
    {
        public int Write(Memory<byte> destination, SelectMethod method)
        {
            var result = destination
                .Write(false); // noWait flag

            return destination.Length - result.Length;
        }
    }
}