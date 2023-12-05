using System;

namespace RabbitMQ.Next.Transport.Methods.Connection;

internal class SecureOkMethodFormatter : IMethodFormatter<SecureOkMethod>
{
    public Span<byte> Write(Span<byte> destination, SecureOkMethod method)
        => destination.Write(method.Response.Span);
}