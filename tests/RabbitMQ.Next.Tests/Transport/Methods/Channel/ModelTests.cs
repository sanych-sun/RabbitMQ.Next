using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods.Channel;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Channel
{
    public class ModelTests
    {
        [Fact]
        public void OpenMethod()
        {
            var method = new OpenMethod();

            Assert.Equal((uint)MethodId.ChannelOpen, method.Method);
        }

        [Fact]
        public void OpenOkMethod()
        {
            var method = new OpenOkMethod();

            Assert.Equal((uint)MethodId.ChannelOpenOk, method.Method);
        }

        [Fact]
        public void FlowMethod()
        {
            var method = new FlowMethod(true);

            Assert.Equal((uint)MethodId.ChannelFlow, method.Method);
            Assert.True(method.Active);
        }

        [Fact]
        public void FlowOkMethod()
        {
            var method = new FlowOkMethod(true);

            Assert.Equal((uint)MethodId.ChannelFlowOk, method.Method);
            Assert.True(method.Active);
        }

        [Fact]
        public void CloseMethod()
        {
            var method = new CloseMethod(ReplyCode.ChannelError, "SomeError", 42);

            Assert.Equal((uint)MethodId.ChannelClose, method.Method);
            Assert.Equal(ReplyCode.ChannelError, method.StatusCode);
            Assert.Equal("SomeError", method.Description);
            Assert.Equal((uint)42, method.FailedMethodUid);
        }

        [Fact]
        public void CloseOkMethod()
        {
            var method = new CloseOkMethod();

            Assert.Equal((uint)MethodId.ChannelCloseOk, method.Method);
        }
    }
}