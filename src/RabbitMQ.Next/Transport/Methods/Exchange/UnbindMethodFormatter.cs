namespace RabbitMQ.Next.Transport.Methods.Exchange;

internal class UnbindMethodFormatter : IMethodFormatter<UnbindMethod>
{
    public void Write(IBufferBuilder destination, UnbindMethod method)
        => destination.Write((short) ProtocolConstants.ObsoleteField)
            .Write(method.Destination)
            .Write(method.Source)
            .Write(method.RoutingKey)
            .Write(false)
            .Write(method.Arguments);
}