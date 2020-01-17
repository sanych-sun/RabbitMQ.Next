using System;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods.Connection;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Connection
{
    public class OpenMethodTests
    {
        [Fact]
        public void OpenMethodCtor()
        {
            var vHost = "/";

            var openMethod = new OpenMethod(vHost);

            Assert.Equal((uint)MethodId.ConnectionOpen, openMethod.Method);
            Assert.Equal(vHost, openMethod.VirtualHost);
        }

        [Fact]
        public void OpenMethodFrameFormatter()
        {
            var expected = Helpers.GetFileContent("RabbitMQ.Next.Tests.Transport.Methods.Connection.OpenMethodPayload.dat");

            var data = new OpenMethod("/");
            Span<byte> payload = stackalloc byte[expected.Length];
            new OpenMethodFrameFormatter().Write(payload, data);

            Assert.Equal(expected, payload.ToArray());
        }
    }
}