namespace RabbitMQ.Next.Transport.Methods.Queue;

internal class DeclareMethodFormatter : IMethodFormatter<DeclareMethod>
{
    public void Write(IBinaryWriter writer, DeclareMethod method)
    {
        writer.Write((short)ProtocolConstants.ObsoleteField);
        writer.Write(method.Queue);
        writer.WriteFlags(method.Passive, method.Durable, method.Exclusive, method.AutoDelete);
        writer.Write(method.Arguments);
    }
}