namespace RabbitMQ.Next.Transport.Methods.Queue;

internal class BindMethodFormatter : IMethodFormatter<BindMethod>
{
    public void Write(IBinaryWriter writer, BindMethod method)
    {
        writer.Write((short)ProtocolConstants.ObsoleteField);
        writer.Write(method.Queue);
        writer.Write(method.Exchange);
        writer.Write(method.RoutingKey);
        writer.Write(false);
        writer.Write(method.Arguments);
    }
}