namespace RabbitMQ.Next.Transport.Methods.Channel;

internal class OpenMethodFormatter : IMethodFormatter<OpenMethod>
{
    public void Write(IBinaryWriter destination, OpenMethod method)
        => destination.Write(ProtocolConstants.ObsoleteField);
}