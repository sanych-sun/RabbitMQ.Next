namespace RabbitMQ.Next.Transport.Methods.Basic;

internal class ConsumeMethodFormatter : IMethodFormatter<ConsumeMethod>
{
    public void Write(IBinaryWriter writer, ConsumeMethod method)
    {
        writer.Write((short)ProtocolConstants.ObsoleteField);
        writer.Write(method.Queue);
        writer.Write(method.ConsumerTag);
        writer.WriteFlags(method.NoLocal, method.NoAck, method.Exclusive);
        writer.Write(method.Arguments);
    }
}