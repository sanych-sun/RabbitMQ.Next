using System;

namespace RabbitMQ.Next.Transport.Methods.Connection;

internal class CloseMethodFormatter : IMethodFormatter<CloseMethod>
{
    public Span<byte> Write(Span<byte> destination, CloseMethod method)
        => destination.Write(method.StatusCode)
            .Write(method.Description)
            .Write((uint) method.FailedMethodId);
}