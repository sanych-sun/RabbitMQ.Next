using System;

namespace RabbitMQ.Next.Transport.Methods.Connection
{
    internal class CloseMethodParser : IMethodParser<CloseMethod>
    {
        public CloseMethod Parse(ReadOnlySpan<byte> payload)
        {
            payload.Read(out ushort status)
                .Read(out string description)
                .Read(out uint methodId);

            return new CloseMethod((ReplyCode)status, description, methodId);
        }

        public IIncomingMethod ParseMethod(ReadOnlySpan<byte> payload) => this.Parse(payload);
    }
}