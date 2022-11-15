namespace RabbitMQ.Next.Transport.Methods.Basic;

internal class AckMethodFormatter : IMethodFormatter<AckMethod>
{
    public void Write(IBufferBuilder destination, AckMethod method)
        => destination
            .Write(method.DeliveryTag)
            .Write(method.Multiple);
}