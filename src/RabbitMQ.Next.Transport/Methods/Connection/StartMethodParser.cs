using System;
using System.Collections.Generic;
using RabbitMQ.Next.Abstractions.Methods;

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

        public IIncomingMethod ParseMethod(ReadOnlySpan<byte> payload) => this.Parse(payload);
    }
}