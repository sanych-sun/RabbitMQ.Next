namespace RabbitMQ.Next.Transport.Methods.Basic;

internal class NackMethodFormatter : IMethodFormatter<NackMethod>
{
    public void Write(IBinaryWriter destination, NackMethod method)
        => destination
            .Write(method.DeliveryTag)
            .WriteFlags(method.Multiple, method.Requeue);
}