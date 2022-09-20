namespace RabbitMQ.Next.Transport.Methods.Queue;

internal class UnbindMethodFormatter : IMethodFormatter<UnbindMethod>
{
    public void Write(IBufferBuilder destination, UnbindMethod method)
        => destination
            .Write((short) ProtocolConstants.ObsoleteField)
            .Write(method.Queue)
            .Write(method.Exchange)
            .Write(method.RoutingKey)
            .Write(method.Arguments);
}