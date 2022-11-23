using System;

namespace RabbitMQ.Next.Transport.Methods.Confirm;

internal class SelectMethodFormatter : IMethodFormatter<SelectMethod>
{
    public Span<byte> Write(Span<byte> destination, SelectMethod method)
        => destination.Write(false); // noWait flag
}