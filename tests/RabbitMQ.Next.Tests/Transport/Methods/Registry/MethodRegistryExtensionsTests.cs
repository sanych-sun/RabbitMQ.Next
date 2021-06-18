using System;
using NSubstitute;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Tests.Mocks;
using RabbitMQ.Next.Transport.Methods;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Registry
{
    public class MethodRegistryExtensionsTests
    {
        [Fact]
        public void FormatMethodThrowsOnUnknownMethod()
        {
            var registry = Substitute.For<IMethodRegistry>();
            registry.GetFormatter<DummyMethod<int>>().Returns((IMethodFormatter<DummyMethod<int>>)null);

            Assert.Throws<NotSupportedException>(() =>
            {
                var res =  registry.FormatMessage(new DummyMethod<int>(MethodId.BasicGet, 1), new byte[100]);
                return res;
            });
        }

        [Fact]
        public void FormatMethod()
        {
            var expected = new byte[]
            {
                0x00, 0x3C, 0x00, 0x46, // methodId
                0x0A, 0x0B, 0x0C, // method args
            };

            var registry = Substitute.For<IMethodRegistry>();
            var dummyFormatter = new DummyFormatter<DummyMethod<int>>(new byte[] { 0x0A, 0x0B, 0x0C});
            registry.GetFormatter<DummyMethod<int>>().Returns(dummyFormatter);
            var buffer = new byte[expected.Length];

            var res = registry.FormatMessage(new DummyMethod<int>(MethodId.BasicGet, 1), buffer);

            Assert.Equal(expected.Length, res);
            Assert.Equal(expected, buffer);
        }
    }
}