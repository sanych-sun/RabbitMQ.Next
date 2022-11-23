using System;

namespace RabbitMQ.Next.Transport.Methods.Basic;

internal class RecoverMethodFormatter : IMethodFormatter<RecoverMethod>
{
    public Span<byte> Write(Span<byte> destination, RecoverMethod method)
        => destination.Write(method.Requeue);
}