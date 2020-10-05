using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods.Channel;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Channel
{
    public class OpenOkMethodTests
    {
        [Fact]
        public void OpenOkMethodCtor()
        {
            var method = new OpenOkMethod();

            Assert.Equal((uint)MethodId.ChannelOpenOk, method.Method);
        }

        [Fact]
        public void OpenOkMethodFormatter()
        {
            var payload = Helpers.GetFileContent("RabbitMQ.Next.Tests.Transport.Methods.Channel.OpenOkMethodPayload.dat");
            var parser = new OpenOkMethodParser();
            var data = parser.Parse(payload);
            var dataBoxed = parser.ParseMethod(payload);

            var expected = new OpenOkMethod();

            Assert.Equal(expected, data);
            Assert.Equal(expected, dataBoxed);
        }
    }
}