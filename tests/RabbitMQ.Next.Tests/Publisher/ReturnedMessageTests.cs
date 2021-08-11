using RabbitMQ.Next.Publisher.Abstractions;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher
{
    public class ReturnedMessageTests
    {
        [Fact]
        public void CtorTest()
        {
            var exchange = "testExchange";
            var routingKey = "routing";
            ushort replyCode = 200;
            var replyText = "OK";

            var message = new ReturnedMessage(exchange, routingKey, replyCode, replyText);

            Assert.Equal(exchange, message.Exchange);
            Assert.Equal(routingKey, message.RoutingKey);
            Assert.Equal(replyCode, message.ReplyCode);
            Assert.Equal(replyText, message.ReplyText);
        }
    }
}