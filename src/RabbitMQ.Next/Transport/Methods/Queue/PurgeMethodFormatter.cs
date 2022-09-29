namespace RabbitMQ.Next.Transport.Methods.Queue;

internal class PurgeMethodFormatter : IMethodFormatter<PurgeMethod>
{
    public void Write(IBinaryWriter writer, PurgeMethod method)
    {
        writer.Write((short)ProtocolConstants.ObsoleteField);
        writer.Write(method.Queue);
        writer.Write(false);
    }
}