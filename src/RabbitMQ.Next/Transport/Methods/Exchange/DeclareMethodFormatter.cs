namespace RabbitMQ.Next.Transport.Methods.Exchange;

internal class DeclareMethodFormatter : IMethodFormatter<DeclareMethod>
{
    public void Write(IBinaryWriter destination, DeclareMethod method)
        => destination
            .Write((short) ProtocolConstants.ObsoleteField)
            .Write(method.Exchange)
            .Write(method.Type)
            .WriteFlags(method.Passive, method.Durable, method.AutoDelete, method.Internal)
            .Write(method.Arguments);
}