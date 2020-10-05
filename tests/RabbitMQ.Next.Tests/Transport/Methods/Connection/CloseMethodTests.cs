using System;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods.Connection;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Connection
{
    public class CloseMethodTests
    {
        [Fact]
        public void CloseMethodCtor()
        {
            var method = new CloseMethod(ReplyCode.ChannelError, "SomeError", 42);

            Assert.Equal((uint)MethodId.ConnectionClose, method.Method);
            Assert.Equal(ReplyCode.ChannelError, method.StatusCode);
            Assert.Equal("SomeError", method.Description);
            Assert.Equal((uint)42, method.FailedMethodUid);
        }

        [Fact]
        public void CloseMethodFormatter()
        {
            var expected = Helpers.GetFileContent("RabbitMQ.Next.Tests.Transport.Methods.Connection.CloseMethodPayload.dat");

            var data = new CloseMethod(ReplyCode.Success, "Goodbye", 0);
            Span<byte> payload = stackalloc byte[expected.Length];
            new CloseMethodFormatter().Write(payload, data);

            Assert.Equal(expected, payload.ToArray());
        }

        [Fact]
        public void CloseMethodFrameParser()
        {
            var payload = Helpers.GetFileContent("RabbitMQ.Next.Tests.Transport.Methods.Connection.CloseMethodPayload.dat");
            var parser = new CloseMethodParser();
            var data = parser.Parse(payload);
            var dataBoxed = parser.ParseMethod(payload);

            var expected = new CloseMethod(ReplyCode.Success, "Goodbye", 0);

            Assert.Equal(expected, data);
            Assert.Equal(expected, dataBoxed);
        }
    }
}