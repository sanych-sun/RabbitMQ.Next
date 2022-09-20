namespace RabbitMQ.Next.Transport.Methods.Connection;

internal class TuneOkMethodFormatter : IMethodFormatter<TuneOkMethod>
{
    public void Write(IBufferBuilder destination, TuneOkMethod method)
        => destination
            .Write(method.ChannelMax)
            .Write(method.MaxFrameSize)
            .Write(method.HeartbeatInterval);
}