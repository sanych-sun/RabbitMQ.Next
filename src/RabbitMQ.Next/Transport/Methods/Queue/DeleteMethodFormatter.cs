namespace RabbitMQ.Next.Transport.Methods.Queue;

internal class DeleteMethodFormatter : IMethodFormatter<DeleteMethod>
{
    public void Write(IBufferBuilder destination, DeleteMethod method)
        => destination
            .Write((short) ProtocolConstants.ObsoleteField)
            .Write(method.Queue)
            .Write(BitConverter.ComposeFlags(method.UnusedOnly, method.EmptyOnly));
}