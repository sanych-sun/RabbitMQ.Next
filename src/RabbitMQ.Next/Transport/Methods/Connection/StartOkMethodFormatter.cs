namespace RabbitMQ.Next.Transport.Methods.Connection;

internal class StartOkMethodFormatter : IMethodFormatter<StartOkMethod>
{
    public void Write(IBufferBuilder destination, StartOkMethod method)
        => destination
            .Write(method.ClientProperties)
            .Write(method.Mechanism)
            .Write(method.Response, true)
            .Write(method.Locale);
}