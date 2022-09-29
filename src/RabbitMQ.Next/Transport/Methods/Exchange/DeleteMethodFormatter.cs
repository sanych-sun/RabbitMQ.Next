namespace RabbitMQ.Next.Transport.Methods.Exchange;

internal class DeleteMethodFormatter : IMethodFormatter<DeleteMethod>
{
    public void Write(IBinaryWriter writer, DeleteMethod method)
    {
        writer.Write((short)ProtocolConstants.ObsoleteField);
        writer.Write(method.Exchange);
        writer.Write(method.UnusedOnly);
    }
}