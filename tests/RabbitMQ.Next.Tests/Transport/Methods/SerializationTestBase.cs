using System;
using System.Collections.Generic;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Transport.Methods.Registry;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods
{
    public abstract class SerializationTestBase
    {
        private readonly string currentNamespace;

        protected SerializationTestBase(Action<IMethodRegistryBuilder> registryBuilder)
        {
            var builder = new MethodRegistryBuilder();
            registryBuilder(builder);
            this.Registry = builder.Build();

            this.currentNamespace = this.GetType().Namespace;
        }

        protected IMethodRegistry Registry { get; }

        protected void TestFormatter<TMethod>(TMethod method)
            where TMethod : struct, IOutgoingMethod
        {
            var payloadResName = $"{this.currentNamespace}.{typeof(TMethod).Name}Payload.dat";
            var formatter = this.Registry.GetFormatter<TMethod>();
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
            var parser = this.Registry.GetParser<TMethod>();
            var payload = Helpers.GetFileContent(payloadResName);

            var data = parser.Parse(payload);
            var dataBoxed = parser.ParseMethod(payload);

            Assert.Equal(method, data, comparer);
            Assert.Equal(method, (TMethod)dataBoxed, comparer);
        }
    }
}