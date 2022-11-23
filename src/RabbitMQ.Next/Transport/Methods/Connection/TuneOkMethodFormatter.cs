using System;

namespace RabbitMQ.Next.Transport.Methods.Connection;

internal class TuneOkMethodFormatter : IMethodFormatter<TuneOkMethod>
{
    public Span<byte> Write(Span<byte> destination, TuneOkMethod method)
        => destination
            .Write(method.ChannelMax)
            .Write(method.MaxFrameSize)
            .Write(method.HeartbeatInterval);
}