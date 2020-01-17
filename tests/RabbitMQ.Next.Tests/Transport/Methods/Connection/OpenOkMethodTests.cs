using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods.Connection;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Connection
{
    public class OpenOkMethodTests
    {
        [Fact]
        public void OpenOkMethodCtor()
        {
            var openOkMethod = new OpenOkMethod();

            Assert.Equal((uint)MethodId.ConnectionOpenOk, openOkMethod.Method);
        }

        [Fact]
        public void OpenOkMethodFrameParser()
        {
            var payload = Helpers.GetFileContent("RabbitMQ.Next.Tests.Transport.Methods.Connection.OpenOkMethodPayload.dat");
            var parser = new OpenOkMethodFrameParser();
            var data = parser.Parse(payload);
            var dataBoxed = parser.ParseMethod(payload);

            var expected = new OpenOkMethod();

            Assert.Equal(expected, data);
            Assert.Equal(expected, dataBoxed);
        }
    }
}