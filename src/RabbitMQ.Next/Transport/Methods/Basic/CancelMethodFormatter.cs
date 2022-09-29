namespace RabbitMQ.Next.Transport.Methods.Basic;

internal class CancelMethodFormatter : IMethodFormatter<CancelMethod>
{
    public void Write(IBinaryWriter writer, CancelMethod method)
    {
        writer.Write(method.ConsumerTag);
        writer.Write(false); // noWait flag
    }
}