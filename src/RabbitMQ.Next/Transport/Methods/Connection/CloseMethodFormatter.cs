using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Connection
{
    internal class CloseMethodFormatter : IMethodFormatter<CloseMethod>
    {
        public int Write(Memory<byte> destination, CloseMethod method)
        {
            var result = destination.Write(method.StatusCode)
                .Write(method.Description)
                .Write((uint) method.FailedMethodId);

            return destination.Length - result.Length;
        }
    }
}