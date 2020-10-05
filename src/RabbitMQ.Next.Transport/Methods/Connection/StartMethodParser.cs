using System;
using System.Collections.Generic;

namespace RabbitMQ.Next.Transport.Methods.Connection
{
    internal class StartMethodParser : IMethodParser<StartMethod>
    {
        public StartMethod Parse(ReadOnlySpan<byte> payload)
        {
            payload.Read(out byte major)
                .Read(out byte minor)
                .Read(out IReadOnlyDictionary<string, object> properties)
                .Read(out var mechanisms, true)
                .Read(out var locales, true);

            return new StartMethod(major, minor, mechanisms, locales, properties);
        }

        public IMethod ParseMethod(ReadOnlySpan<byte> payload) => this.Parse(payload);
    }
}