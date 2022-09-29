namespace RabbitMQ.Next.Transport.Methods.Basic;

internal class RecoverMethodFormatter : IMethodFormatter<RecoverMethod>
{
    public void Write(IBinaryWriter writer, RecoverMethod method)
     => writer.Write(method.Requeue);
}