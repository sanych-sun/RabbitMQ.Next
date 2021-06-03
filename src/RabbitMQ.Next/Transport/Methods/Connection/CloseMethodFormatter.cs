using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Connection
{
    internal class CloseMethodFormatter : IMethodFormatter<CloseMethod>
    {
        public Span<byte> Write(Span<byte> destination, CloseMethod method) =>
            destination.Write(method.StatusCode)
                .Write(method.Description)
                .Write(method.FailedMethodId);
    }
}