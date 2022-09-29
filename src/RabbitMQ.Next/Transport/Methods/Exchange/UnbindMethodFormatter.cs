namespace RabbitMQ.Next.Transport.Methods.Exchange;

internal class UnbindMethodFormatter : IMethodFormatter<UnbindMethod>
{
    public void Write(IBinaryWriter writer, UnbindMethod method)
    {
        writer.Write((short)ProtocolConstants.ObsoleteField);
        writer.Write(method.Destination);
        writer.Write(method.Source);
        writer.Write(method.RoutingKey);
        writer.Write(false);
        writer.Write(method.Arguments);
    }
}