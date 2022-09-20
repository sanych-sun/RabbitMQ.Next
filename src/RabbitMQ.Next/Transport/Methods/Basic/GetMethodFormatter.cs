namespace RabbitMQ.Next.Transport.Methods.Basic;

internal class GetMethodFormatter : IMethodFormatter<GetMethod>
{
    public void Write(IBufferBuilder destination, GetMethod method)
        => destination
            .Write((short) ProtocolConstants.ObsoleteField)
            .Write(method.Queue)
            .Write(method.NoAck);
}