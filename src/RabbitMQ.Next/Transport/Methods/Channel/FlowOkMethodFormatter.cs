namespace RabbitMQ.Next.Transport.Methods.Channel;

internal class FlowOkMethodFormatter : IMethodFormatter<FlowOkMethod>
{
    public void Write(IBinaryWriter writer, FlowOkMethod method)
        => writer.Write(method.Active);
}