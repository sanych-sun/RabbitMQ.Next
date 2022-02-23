using System;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Connection
{
    internal class TuneOkMethodFormatter : IMethodFormatter<TuneOkMethod>
    {
        public int Write(Span<byte> destination, TuneOkMethod method)
        {
            var result = destination
                .Write(method.ChannelMax)
                .Write(method.MaxFrameSize)
                .Write(method.HeartbeatInterval);

            return destination.Length - result.Length;
        }
    }
}