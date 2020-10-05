using System;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods.Channel;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Channel
{
    public class FlowMethodTests
    {
        [Fact]
        public void FlowMethodCtor()
        {
            var method = new FlowMethod(true);

            Assert.Equal((uint)MethodId.ChannelFlow, method.Method);
            Assert.Equal(true, method.Active);
        }

        [Fact]
        public void FlowMethodFormatter()
        {
            var expected = Helpers.GetFileContent("RabbitMQ.Next.Tests.Transport.Methods.Channel.FlowMethodPayload.dat");

            var data = new FlowMethod(true);
            Span<byte> payload = stackalloc byte[expected.Length];
            new FlowMethodFormatter().Write(payload, data);

            Assert.Equal(expected, payload.ToArray());
        }

        [Fact]
        public void FlowMethodFrameParser()
        {
            var payload = Helpers.GetFileContent("RabbitMQ.Next.Tests.Transport.Methods.Channel.FlowMethodPayload.dat");
            var parser = new FlowMethodParser();
            var data = parser.Parse(payload);
            var dataBoxed = parser.ParseMethod(payload);

            var expected = new FlowMethod(true);

            Assert.Equal(expected, data);
            Assert.Equal(expected, dataBoxed);
        }
    }
}