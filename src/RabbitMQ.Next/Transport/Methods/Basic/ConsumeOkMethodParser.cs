using System;

namespace RabbitMQ.Next.Transport.Methods.Basic;

internal class ConsumeOkMethodParser : IMethodParser<ConsumeOkMethod>
{
    public ConsumeOkMethod Parse(ReadOnlySpan<byte> payload)
    {
        payload.Read(out string consumerTag);

        return new ConsumeOkMethod(consumerTag);
    }
}