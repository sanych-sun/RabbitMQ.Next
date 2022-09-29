namespace RabbitMQ.Next.Transport.Methods.Basic;

internal class PublishMethodFormatter : IMethodFormatter<PublishMethod>
{
    public void Write(IBinaryWriter writer, PublishMethod method)
    {
        writer.Write((short)ProtocolConstants.ObsoleteField);
        writer.Write(method.Exchange);
        writer.Write(method.RoutingKey);
        writer.WriteFlags(method.Mandatory, method.Immediate);
    }
}