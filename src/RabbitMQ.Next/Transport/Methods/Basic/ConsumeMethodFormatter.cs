namespace RabbitMQ.Next.Transport.Methods.Basic;

internal class ConsumeMethodFormatter : IMethodFormatter<ConsumeMethod>
{
    public void Write(IBinaryWriter destination, ConsumeMethod method) 
        => destination
            .Write((short) ProtocolConstants.ObsoleteField)
            .Write(method.Queue)
            .Write(method.ConsumerTag)
            .WriteFlags(method.NoLocal, method.NoAck, method.Exclusive)
            .Write(method.Arguments);
}