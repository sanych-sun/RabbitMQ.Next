using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Connection
{
    internal class TuneMethodParser : IMethodParser<TuneMethod>
    {
        public TuneMethod Parse(ReadOnlyMemory<byte> payload)
        {
            payload.Read(out ushort channels)
                .Read(out uint frameMax)
                .Read(out ushort heartbeatInterval);

            return new TuneMethod(channels, frameMax, heartbeatInterval);
        }

        public IIncomingMethod ParseMethod(ReadOnlyMemory<byte> payload) => this.Parse(payload);
    }
}