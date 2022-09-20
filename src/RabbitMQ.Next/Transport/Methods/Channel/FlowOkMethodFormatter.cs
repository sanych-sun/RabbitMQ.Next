namespace RabbitMQ.Next.Transport.Methods.Channel;

internal class FlowOkMethodFormatter : IMethodFormatter<FlowOkMethod>
{
    public void Write(IBinaryWriter destination, FlowOkMethod method)
        => destination.Write(method.Active);
}