namespace RabbitMQ.Next.Transport.Methods.Basic;

internal class PublishMethodFormatter : IMethodFormatter<PublishMethod>
{
    public void Write(IBufferBuilder destination, PublishMethod method) 
        => destination
            .Write((short) ProtocolConstants.ObsoleteField)
            .Write(method.Exchange)
            .Write(method.RoutingKey)
            .Write(method.Flags);
}