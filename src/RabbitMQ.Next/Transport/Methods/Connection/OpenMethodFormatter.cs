namespace RabbitMQ.Next.Transport.Methods.Connection;

internal class OpenMethodFormatter : IMethodFormatter<OpenMethod>
{
    public void Write(IBinaryWriter destination, OpenMethod method)
        => destination
            .Write(method.VirtualHost)
            .Write(ProtocolConstants.ObsoleteField)
            .Write(ProtocolConstants.ObsoleteField);
}