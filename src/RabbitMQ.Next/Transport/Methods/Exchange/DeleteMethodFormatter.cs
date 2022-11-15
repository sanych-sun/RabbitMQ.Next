namespace RabbitMQ.Next.Transport.Methods.Exchange;

internal class DeleteMethodFormatter : IMethodFormatter<DeleteMethod>
{
    public void Write(IBinaryWriter destination, DeleteMethod method)
        => destination.Write((short) ProtocolConstants.ObsoleteField)
            .Write(method.Exchange)
            .Write(method.UnusedOnly);
}