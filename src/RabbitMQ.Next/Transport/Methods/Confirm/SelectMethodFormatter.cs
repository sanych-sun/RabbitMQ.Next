namespace RabbitMQ.Next.Transport.Methods.Confirm;

internal class SelectMethodFormatter : IMethodFormatter<SelectMethod>
{
    public void Write(IBinaryWriter destination, SelectMethod method)
        => destination.Write(false); // noWait flag
}