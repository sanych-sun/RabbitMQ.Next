using System.Collections.Generic;
using RabbitMQ.Next.Methods;
using RabbitMQ.Next.Tests.Mocks;
using RabbitMQ.Next.Transport.Methods;
using RabbitMQ.Next.Transport.Methods.Registry;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods;

public abstract class SerializationTestBase
{
    private readonly string currentNamespace;

    protected SerializationTestBase()
    {
        this.Registry = new MethodRegistryBuilder()
            .AddConnectionMethods()
            .AddChannelMethods()
            .AddExchangeMethods()
            .AddQueueMethods()
            .AddBasicMethods()
            .AddConfirmMethods()
            .Build();

        this.currentNamespace = this.GetType().Namespace;
    }

    private IMethodRegistry Registry { get; }

    protected void TestFormatter<TMethod>(TMethod method)
        where TMethod : struct, IOutgoingMethod
    {
        var payloadResName = $"{this.currentNamespace}.{typeof(TMethod).Name}Payload.dat";
        var formatter = this.Registry.GetFormatter<TMethod>();
        var expected = Helpers.GetFileContent(payloadResName);

        var buffer =  new BinaryWriterMock(expected.Length);
        formatter.Write(buffer, method);

        Assert.Equal(expected, buffer.Written.ToArray());
    }

    protected void TestParser<TMethod>(TMethod method, IEqualityComparer<TMethod> comparer = null)
        where TMethod : struct, IIncomingMethod
    {
        comparer ??= EqualityComparer<TMethod>.Default;

        var payloadResName = $"{this.currentNamespace}.{typeof(TMethod).Name}Payload.dat";
        var parser = this.Registry.GetParser<TMethod>();
        var payload = Helpers.GetFileContent(payloadResName);

        var data = parser.Parse(payload);

        Assert.Equal(method, data, comparer);
    }
}