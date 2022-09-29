namespace RabbitMQ.Next.Transport.Methods.Channel;

internal class FlowMethodFormatter : IMethodFormatter<FlowMethod>
{
    public void Write(IBinaryWriter writer, FlowMethod method)
        => writer.Write(method.Active);
}