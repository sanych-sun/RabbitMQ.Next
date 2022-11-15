namespace RabbitMQ.Next.Transport.Methods.Basic;

internal class PublishMethodFormatter : IMethodFormatter<PublishMethod>
{
    public void Write(IBinaryWriter destination, PublishMethod method) 
        => destination
            .Write((short) ProtocolConstants.ObsoleteField)
            .Write(method.Exchange)
            .Write(method.RoutingKey)
            .WriteFlags(method.Mandatory, method.Immediate);
}