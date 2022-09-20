namespace RabbitMQ.Next.Transport.Methods.Queue;

internal class DeleteMethodFormatter : IMethodFormatter<DeleteMethod>
{
    public void Write(IBinaryWriter destination, DeleteMethod method)
        => destination
            .Write((short) ProtocolConstants.ObsoleteField)
            .Write(method.Queue)
            .WriteFlags(method.UnusedOnly, method.EmptyOnly);
}