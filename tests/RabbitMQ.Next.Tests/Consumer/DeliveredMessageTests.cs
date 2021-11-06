using RabbitMQ.Next.Consumer.Abstractions;
using Xunit;

namespace RabbitMQ.Next.Tests.Consumer
{
    public class DeliveredMessageTests
    {
        [Fact]
        public void DeliveredMessage()
        {
            var exchange = "exchange";
            var routingKey = "routingKey";
            var redelivered = true;
            var consumerTag = "consumerTag";
            var deliveryTag = (ulong)42;

            var message = new DeliveredMessage(exchange, routingKey, redelivered, consumerTag, deliveryTag);

            Assert.Equal(exchange, message.Exchange);
            Assert.Equal(routingKey, message.RoutingKey);
            Assert.Equal(redelivered, message.Redelivered);
            Assert.Equal(consumerTag, message.ConsumerTag);
            Assert.Equal(deliveryTag, message.DeliveryTag);
        }
    }
}