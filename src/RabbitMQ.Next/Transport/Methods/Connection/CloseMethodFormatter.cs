namespace RabbitMQ.Next.Transport.Methods.Connection;

internal class CloseMethodFormatter : IMethodFormatter<CloseMethod>
{
    public void Write(IBinaryWriter destination, CloseMethod method)
        => destination.Write(method.StatusCode)
            .Write(method.Description)
            .Write((uint) method.FailedMethodId);
}