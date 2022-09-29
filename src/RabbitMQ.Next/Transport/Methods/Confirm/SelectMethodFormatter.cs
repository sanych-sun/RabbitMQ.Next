namespace RabbitMQ.Next.Transport.Methods.Confirm;

internal class SelectMethodFormatter : IMethodFormatter<SelectMethod>
{
    public void Write(IBinaryWriter writer, SelectMethod method)
        => writer.Write(false); // noWait flag
}