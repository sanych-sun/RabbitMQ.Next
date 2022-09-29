namespace RabbitMQ.Next.Transport.Methods.Basic;

internal class AckMethodFormatter : IMethodFormatter<AckMethod>
{
    public void Write(IBinaryWriter writer, AckMethod method)
    {
        writer.Write(method.DeliveryTag);
        writer.Write(method.Multiple);
    }
}