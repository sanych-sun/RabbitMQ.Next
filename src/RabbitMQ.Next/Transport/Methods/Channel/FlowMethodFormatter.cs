namespace RabbitMQ.Next.Transport.Methods.Channel;

internal class FlowMethodFormatter : IMethodFormatter<FlowMethod>
{
    public void Write(IBufferBuilder destination, FlowMethod method)
        => destination.Write(method.Active);
}