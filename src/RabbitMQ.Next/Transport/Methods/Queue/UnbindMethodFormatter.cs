namespace RabbitMQ.Next.Transport.Methods.Queue;

internal class UnbindMethodFormatter : IMethodFormatter<UnbindMethod>
{
    public void Write(IBinaryWriter writer, UnbindMethod method)
    {
        writer.Write((short)ProtocolConstants.ObsoleteField);
        writer.Write(method.Queue);
        writer.Write(method.Exchange);
        writer.Write(method.RoutingKey);
        writer.Write(method.Arguments);
    }
}