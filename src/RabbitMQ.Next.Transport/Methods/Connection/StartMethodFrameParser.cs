using System;
using System.Collections.Generic;

namespace RabbitMQ.Next.Transport.Methods.Connection
{
    internal class StartMethodFrameParser : IMethodFrameParser<StartMethod>
    {
        public StartMethod Parse(ReadOnlySpan<byte> payload)
        {
            payload.Read(out byte major)
                .Read(out byte minor)
                .Read(out IReadOnlyDictionary<string, object> properties)
                .Read(out string mechanisms, true)
                .Read(out string locales, true);

            return new StartMethod(major, minor, mechanisms, locales, properties);
        }

        public IMethod ParseMethod(ReadOnlySpan<byte> payload) => this.Parse(payload);
    }
}