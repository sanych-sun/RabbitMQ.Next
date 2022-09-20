namespace RabbitMQ.Next.Transport.Methods.Queue;

internal class DeclareMethodFormatter : IMethodFormatter<DeclareMethod>
{
    public void Write(IBufferBuilder destination, DeclareMethod method)
        => destination
            .Write((short) ProtocolConstants.ObsoleteField)
            .Write(method.Queue)
            .Write(BitConverter.ComposeFlags(method.Passive, method.Durable, method.Exclusive, method.AutoDelete))
            .Write(method.Arguments);
}