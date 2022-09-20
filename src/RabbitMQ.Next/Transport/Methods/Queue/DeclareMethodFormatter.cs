namespace RabbitMQ.Next.Transport.Methods.Queue;

internal class DeclareMethodFormatter : IMethodFormatter<DeclareMethod>
{
    public void Write(IBinaryWriter destination, DeclareMethod method)
        => destination
            .Write((short) ProtocolConstants.ObsoleteField)
            .Write(method.Queue)
            .WriteFlags(method.Passive, method.Durable, method.Exclusive, method.AutoDelete)
            .Write(method.Arguments);
}