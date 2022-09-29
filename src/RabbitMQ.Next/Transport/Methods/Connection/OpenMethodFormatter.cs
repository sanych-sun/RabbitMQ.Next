namespace RabbitMQ.Next.Transport.Methods.Connection;

internal class OpenMethodFormatter : IMethodFormatter<OpenMethod>
{
    public void Write(IBinaryWriter writer, OpenMethod method)
    {
        writer.Write(method.VirtualHost);
        writer.Write(ProtocolConstants.ObsoleteField);
        writer.Write(ProtocolConstants.ObsoleteField);
    }
}