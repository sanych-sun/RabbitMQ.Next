namespace RabbitMQ.Next.Transport.Methods.Basic;

internal class QosMethodFormatter : IMethodFormatter<QosMethod>
{
    public void Write(IBinaryWriter writer, QosMethod method)
    {
        writer.Write(method.PrefetchSize);
        writer.Write(method.PrefetchCount);
        writer.Write(method.Global);
    }
}