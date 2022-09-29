namespace RabbitMQ.Next.Transport.Methods.Connection;

internal class TuneOkMethodFormatter : IMethodFormatter<TuneOkMethod>
{
    public void Write(IBinaryWriter writer, TuneOkMethod method)
    {
        writer.Write(method.ChannelMax);
        writer.Write(method.MaxFrameSize);
        writer.Write(method.HeartbeatInterval);
    }
}