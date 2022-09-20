namespace RabbitMQ.Next.Transport.Methods.Basic;

internal class GetMethodFormatter : IMethodFormatter<GetMethod>
{
    public void Write(IBinaryWriter destination, GetMethod method)
        => destination
            .Write((short) ProtocolConstants.ObsoleteField)
            .Write(method.Queue)
            .Write(method.NoAck);
}