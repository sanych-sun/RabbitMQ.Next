namespace RabbitMQ.Next.Transport.Methods.Channel;

internal class OpenMethodFormatter : IMethodFormatter<OpenMethod>
{
    public void Write(IBinaryWriter writer, OpenMethod method)
        => writer.Write(ProtocolConstants.ObsoleteField);
}