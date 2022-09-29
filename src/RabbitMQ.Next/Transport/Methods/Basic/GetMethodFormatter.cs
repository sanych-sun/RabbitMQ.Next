namespace RabbitMQ.Next.Transport.Methods.Basic;

internal class GetMethodFormatter : IMethodFormatter<GetMethod>
{
    public void Write(IBinaryWriter writer, GetMethod method)
    {
        writer.Write((short)ProtocolConstants.ObsoleteField);
        writer.Write(method.Queue);
        writer.Write(method.NoAck);
    }
}