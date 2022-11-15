namespace RabbitMQ.Next.Transport.Methods.Basic;

internal class ConsumeMethodFormatter : IMethodFormatter<ConsumeMethod>
{
    public void Write(IBufferBuilder destination, ConsumeMethod method) 
        => destination
            .Write((short) ProtocolConstants.ObsoleteField)
            .Write(method.Queue)
            .Write(method.ConsumerTag)
            .Write(method.Flags)
            .Write(method.Arguments);
}