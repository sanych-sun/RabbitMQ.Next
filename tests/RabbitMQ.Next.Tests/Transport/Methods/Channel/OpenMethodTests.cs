using System;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods.Channel;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Channel
{
    public class OpenMethodTests
    {
        [Fact]
        public void OpenMethodCtor()
        {
            var method = new OpenMethod();

            Assert.Equal((uint)MethodId.ChannelOpen, method.Method);
        }

        [Fact]
        public void OpenMethodFormatter()
        {
            var expected = Helpers.GetFileContent("RabbitMQ.Next.Tests.Transport.Methods.Channel.OpenMethodPayload.dat");
            var data = new OpenMethod();

            Span<byte> payload = stackalloc byte[expected.Length];
            new OpenMethodFormatter().Write(payload, data);

            Assert.Equal(expected, payload.ToArray());
        }
    }
}