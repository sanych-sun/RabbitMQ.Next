namespace RabbitMQ.Next.Transport.Methods.Basic;

internal class QosMethodFormatter : IMethodFormatter<QosMethod>
{
    public void Write(IBinaryWriter destination, QosMethod method)
        => destination
        .Write(method.PrefetchSize)
        .Write(method.PrefetchCount)
        .Write(method.Global);
}