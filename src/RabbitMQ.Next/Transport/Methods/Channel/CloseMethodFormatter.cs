namespace RabbitMQ.Next.Transport.Methods.Channel;

internal class CloseMethodFormatter : IMethodFormatter<CloseMethod>
{
    public void Write(IBinaryWriter writer, CloseMethod method)
    {
        writer.Write(method.StatusCode);
        writer.Write(method.Description);
        writer.Write((uint)method.FailedMethodId);
    }
}