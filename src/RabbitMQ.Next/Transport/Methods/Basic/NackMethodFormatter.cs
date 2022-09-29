namespace RabbitMQ.Next.Transport.Methods.Basic;

internal class NackMethodFormatter : IMethodFormatter<NackMethod>
{
    public void Write(IBinaryWriter writer, NackMethod method)
    {
        writer.Write(method.DeliveryTag);
        writer.WriteFlags(method.Multiple, method.Requeue);
    }
}