using System;
using System.Collections.Generic;

namespace RabbitMQ.Next.Transport.Methods.Connection;

internal class SecureMethodParser : IMethodParser<SecureMethod>
{
    public SecureMethod Parse(ReadOnlySpan<byte> payload)
    {
        payload.Read(out byte[] challenge);

        return new SecureMethod(challenge);
    }
}