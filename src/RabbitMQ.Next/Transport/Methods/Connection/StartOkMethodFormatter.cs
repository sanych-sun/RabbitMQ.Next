using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Connection
{
    internal class StartOkMethodFormatter : IMethodFormatter<StartOkMethod>
    {
        public int Write(Memory<byte> destination, StartOkMethod method)
        {
            var result = destination
                .Write(method.ClientProperties)
                .Write(method.Mechanism)
                .Write(method.Response, true)
                .Write(method.Locale);

            return destination.Length - result.Length;
        }
    }
}