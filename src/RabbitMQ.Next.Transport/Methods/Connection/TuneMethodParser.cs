using System;

namespace RabbitMQ.Next.Transport.Methods.Connection
{
    internal class TuneMethodParser : IMethodParser<TuneMethod>
    {
        public TuneMethod Parse(ReadOnlySpan<byte> payload)
        {
            payload.Read(out ushort channels)
                .Read(out uint frameMax)
                .Read(out ushort heartbeatInterval);

            return new TuneMethod(channels, frameMax, heartbeatInterval);
        }

        public IMethod ParseMethod(ReadOnlySpan<byte> payload) => this.Parse(payload);
    }
}