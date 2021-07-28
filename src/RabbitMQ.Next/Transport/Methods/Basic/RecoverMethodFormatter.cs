using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic
{
    internal class RecoverMethodFormatter : IMethodFormatter<RecoverMethod>
    {
        public int Write(Memory<byte> destination, RecoverMethod method)
        {
            var result = destination.Write(method.Requeue);

            return destination.Length - result.Length;
        }
    }
}