namespace RabbitMQ.Next.Transport.Methods.Basic;

internal class CancelMethodFormatter : IMethodFormatter<CancelMethod>
{
    public void Write(IBufferBuilder destination, CancelMethod method)
        => destination
            .Write(method.ConsumerTag)
            .Write(false); // noWait flag
}