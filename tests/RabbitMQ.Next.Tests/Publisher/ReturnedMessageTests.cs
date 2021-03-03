using RabbitMQ.Next.Abstractions.Messaging;
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
            IMessageProperties properties = new MessageProperties();
            ushort replyCode = 200;
            var replyText = "OK";

            var message = new ReturnedMessage(exchange, routingKey, properties, replyCode, replyText);

            Assert.Equal(exchange, message.Exchange);
            Assert.Equal(routingKey, message.RoutingKey);
            Assert.Equal(properties, message.Properties);
            Assert.Equal(replyCode, message.ReplyCode);
            Assert.Equal(replyText, message.ReplyText);
        }
    }
}