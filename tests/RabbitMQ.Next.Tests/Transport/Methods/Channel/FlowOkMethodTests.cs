using System;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods.Channel;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Channel
{
    public class FlowOkMethodTests
    {
        [Fact]
        public void FlowOkMethodCtor()
        {
            var method = new FlowOkMethod(true);

            Assert.Equal((uint)MethodId.ChannelFlowOk, method.Method);
            Assert.Equal(true, method.Active);
        }

        [Fact]
        public void FlowOkMethodFormatter()
        {
            var expected = Helpers.GetFileContent("RabbitMQ.Next.Tests.Transport.Methods.Channel.FlowOkMethodPayload.dat");

            var data = new FlowOkMethod(true);
            Span<byte> payload = stackalloc byte[expected.Length];
            new FlowOkMethodFormatter().Write(payload, data);

            Assert.Equal(expected, payload.ToArray());
        }

        [Fact]
        public void FlowOkMethodFrameParser()
        {
            var payload = Helpers.GetFileContent("RabbitMQ.Next.Tests.Transport.Methods.Channel.FlowOkMethodPayload.dat");
            var parser = new FlowOkMethodParser();
            var data = parser.Parse(payload);
            var dataBoxed = parser.ParseMethod(payload);

            var expected = new FlowOkMethod(true);

            Assert.Equal(expected, data);
            Assert.Equal(expected, dataBoxed);
        }
    }
}