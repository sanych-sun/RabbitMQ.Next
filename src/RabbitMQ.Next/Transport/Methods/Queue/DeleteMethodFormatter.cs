namespace RabbitMQ.Next.Transport.Methods.Queue;

internal class DeleteMethodFormatter : IMethodFormatter<DeleteMethod>
{
    public void Write(IBinaryWriter writer, DeleteMethod method)
    {
        writer.Write((short)ProtocolConstants.ObsoleteField);
        writer.Write(method.Queue);
        writer.WriteFlags(method.UnusedOnly, method.EmptyOnly);
    }
}