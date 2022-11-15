namespace RabbitMQ.Next.Transport.Methods.Queue;

internal class PurgeMethodFormatter : IMethodFormatter<PurgeMethod>
{
    public void Write(IBufferBuilder destination, PurgeMethod method)
        => destination
            .Write((short) ProtocolConstants.ObsoleteField)
            .Write(method.Queue)
            .Write(false);
}