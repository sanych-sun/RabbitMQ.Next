using System;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Channel;

internal class OpenMethodFormatter : IMethodFormatter<OpenMethod>
{
    public int Write(Span<byte> destination, OpenMethod method)
    {
        var result = destination.Write(ProtocolConstants.ObsoleteField);
            
        return destination.Length - result.Length;
    }
}