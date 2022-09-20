namespace RabbitMQ.Next.Transport.Methods.Basic;

internal class RecoverMethodFormatter : IMethodFormatter<RecoverMethod>
{
    public void Write(IBufferBuilder destination, RecoverMethod method)
     => destination.Write(method.Requeue);
}