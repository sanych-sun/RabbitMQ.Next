using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods.Basic;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Basic
{
    public class ModelTests
    {
        [Fact]
        public void PublishMethod()
        {
            var exchange = "exchange";
            var routingKey = "routing";
            var flags = (byte)0b_00000001;

            var method = new PublishMethod(exchange, routingKey, flags);

            Assert.Equal((uint)MethodId.BasicPublish, method.Method);
            Assert.Equal(exchange, method.Exchange);
            Assert.Equal(routingKey, method.RoutingKey);
            Assert.Equal(flags, method.Flags);
        }

        [Theory]
        [InlineData(0b_00000000, false, false)]
        [InlineData(0b_00000001, true, false)]
        [InlineData(0b_00000010, false, true)]
        [InlineData(0b_00000011, true, true)]
        public void PublishMethodFlags(byte expected, bool mandatory, bool immediate)
        {
            var method = new PublishMethod("exchange", "routing", mandatory, immediate);

            Assert.Equal(expected, method.Flags);
        }

        [Fact]
        public void ReturnMethod()
        {
            var exchange = "exchange";
            var routingKey = "routing";
            var replyCode = (ushort)400;
            var replyText = "some error";

            var method = new ReturnMethod(exchange, routingKey, replyCode, replyText);

            Assert.Equal((uint)MethodId.BasicReturn, method.Method);
            Assert.Equal(exchange, method.Exchange);
            Assert.Equal(routingKey, method.RoutingKey);
            Assert.Equal(replyCode, method.ReplyCode);
            Assert.Equal(replyText, method.ReplyText);
        }
    }
}