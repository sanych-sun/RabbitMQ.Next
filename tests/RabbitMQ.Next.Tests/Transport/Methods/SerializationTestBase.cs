using System;
using System.Collections.Generic;
using RabbitMQ.Next.Methods;
using RabbitMQ.Next.Tests.Mocks;
using RabbitMQ.Next.Transport.Methods;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods;

public abstract class SerializationTestBase
{
    private readonly string currentNamespace;

    protected SerializationTestBase()
    {
        this.currentNamespace = this.GetType().Namespace;
    }
    
    protected void TestFormatter<TMethod>(TMethod method)
        where TMethod : struct, IOutgoingMethod
    {
        var payloadResName = $"{this.currentNamespace}.{typeof(TMethod).Name}Payload.dat";
        var formatter = MethodRegistry.GetFormatter<TMethod>();
        var expected = Helpers.GetFileContent(payloadResName);

        Span<byte> payload = stackalloc byte[expected.Length];
        formatter.Write(payload, method);

        Assert.Equal(expected, payload.ToArray());
    }

    protected void TestParser<TMethod>(TMethod method, IEqualityComparer<TMethod> comparer = null)
        where TMethod : struct, IIncomingMethod
    {
        comparer ??= EqualityComparer<TMethod>.Default;

        var payloadResName = $"{this.currentNamespace}.{typeof(TMethod).Name}Payload.dat";
        var parser = MethodRegistry.GetParser<TMethod>();
        var payload = Helpers.GetFileContent(payloadResName);

        var data = parser.Parse(payload);

        Assert.Equal(method, data, comparer);
    }
}