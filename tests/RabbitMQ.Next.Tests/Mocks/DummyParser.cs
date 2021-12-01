using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Tests.Mocks
{
    public class DummyParser<TMethod> : IMethodParser<TMethod>
        where TMethod: struct, IIncomingMethod
    {
        public TMethod Parse(ReadOnlySpan<byte> payload) => default;
    }
}