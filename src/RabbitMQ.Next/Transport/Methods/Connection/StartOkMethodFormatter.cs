namespace RabbitMQ.Next.Transport.Methods.Connection;

internal class StartOkMethodFormatter : IMethodFormatter<StartOkMethod>
{
    public void Write(IBinaryWriter writer, StartOkMethod method)
    {
        writer.Write(method.ClientProperties);
        writer.Write(method.Mechanism);
        writer.Write(method.Response, true);
        writer.Write(method.Locale);
    }
}