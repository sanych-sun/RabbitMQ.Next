namespace RabbitMQ.Next.Transport.Methods.Basic;

internal class NackMethodFormatter : IMethodFormatter<NackMethod>
{
    public void Write(IBufferBuilder destination, NackMethod method)
        => destination
            .Write(method.DeliveryTag)
            .Write(BitConverter.ComposeFlags(method.Multiple, method.Requeue));
}