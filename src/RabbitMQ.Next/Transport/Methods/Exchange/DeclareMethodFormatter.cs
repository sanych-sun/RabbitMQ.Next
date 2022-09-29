namespace RabbitMQ.Next.Transport.Methods.Exchange;

internal class DeclareMethodFormatter : IMethodFormatter<DeclareMethod>
{
    public void Write(IBinaryWriter writer, DeclareMethod method)
    {
        writer.Write((short)ProtocolConstants.ObsoleteField);
        writer.Write(method.Exchange);
        writer.Write(method.Type);
        writer.WriteFlags(method.Passive, method.Durable, method.AutoDelete, method.Internal);
        writer.Write(method.Arguments);
    }
}