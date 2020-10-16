using System.Collections.Generic;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods.Queue;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Queue
{
    public class ModelTests
    {
        [Fact]
        public void DeclareMethod()
        {
            var name = "queueName";
            var flags = (byte)42;
            var arguments = new Dictionary<string, object>()
            {
                ["a"] = "a",
            };

            var method = new DeclareMethod(name, flags, arguments);

            Assert.Equal((uint)MethodId.QueueDeclare, method.Method);
            Assert.Equal(name, method.Queue);
            Assert.Equal(flags, method.Flags);
            Assert.Equal(arguments, method.Arguments);
        }

        [Theory]
        [InlineData(0b_00000000, false, false, false, false, false)]
        [InlineData(0b_00000001, true, false, false, false, false)]
        [InlineData(0b_00000010, false, true, false, false, false)]
        [InlineData(0b_00000100, false, false, true, false, false)]
        [InlineData(0b_00001000, false, false, false, true, false)]
        [InlineData(0b_00010000, false, false, false, false, true)]
        [InlineData(0b_00011111, true, true, true, true, true)]
        public void DeclareMethodFlags(byte expected, bool passive, bool durable, bool exclusive, bool autoDelete, bool nowait)
        {
            var method = new DeclareMethod("queue", passive, durable, exclusive, autoDelete, nowait, null);

            Assert.Equal(expected, method.Flags);
        }

        [Fact]
        public void DeclareOkMethod()
        {
            var name = "queueName";
            uint messageCount = 10;
            uint consumerCount = 20;

            var method = new DeclareOkMethod(name, messageCount, consumerCount);

            Assert.Equal((uint)MethodId.QueueDeclareOk, method.Method);
            Assert.Equal(name, method.Queue);
            Assert.Equal(messageCount, method.MessageCount);
            Assert.Equal(consumerCount, method.ConsumerCount);
        }

        [Fact]
        public void BindMethod()
        {
            var queue = "destination";
            var exchange = "source";
            var routingKey = "routingKey";
            var arguments = new Dictionary<string, object>()
            {
                ["a"] = "a",
            };

            var method = new BindMethod(queue, exchange, routingKey, arguments);

            Assert.Equal((uint)MethodId.QueueBind, method.Method);
            Assert.Equal(queue, method.Queue);
            Assert.Equal(exchange, method.Exchange);
            Assert.Equal(routingKey, method.RoutingKey);
            Assert.Equal(arguments, method.Arguments);
        }

        [Fact]
        public void BindOkMethod()
        {
            var method = new BindOkMethod();

            Assert.Equal((uint)MethodId.QueueBindOk, method.Method);
        }

        [Fact]
        public void UnbindMethod()
        {
            var queue = "destination";
            var exchange = "source";
            var routingKey = "routingKey";
            var arguments = new Dictionary<string, object>()
            {
                ["a"] = "a",
            };

            var method = new UnbindMethod(queue, exchange, routingKey, arguments);

            Assert.Equal((uint)MethodId.QueueUnbind, method.Method);
            Assert.Equal(queue, method.Queue);
            Assert.Equal(exchange, method.Exchange);
            Assert.Equal(routingKey, method.RoutingKey);
            Assert.Equal(arguments, method.Arguments);
        }

        [Fact]
        public void UnbindOkMethod()
        {
            var method = new UnbindOkMethod();

            Assert.Equal((uint)MethodId.QueueUnbindOk, method.Method);
        }

        [Fact]
        public void PurgeMethod()
        {
            var queue = "destination";

            var method = new PurgeMethod(queue);

            Assert.Equal((uint)MethodId.QueuePurge, method.Method);
            Assert.Equal(queue, method.Queue);
        }

        [Fact]
        public void PurgeOkMethod()
        {
            uint messageCount = 10;

            var method = new PurgeOkMethod(messageCount);

            Assert.Equal((uint)MethodId.QueuePurgeOk, method.Method);
            Assert.Equal(messageCount, method.MessageCount);
        }
        
        [Fact]
        public void DeleteMethod()
        {
            var queue = "destination";
            var unusedOnly = true;
            var emptyOnly = true;

            var method = new DeleteMethod(queue, unusedOnly, emptyOnly);

            Assert.Equal((uint)MethodId.QueueDelete, method.Method);
            Assert.Equal(queue, method.Queue);
            Assert.Equal(unusedOnly, method.UnusedOnly);
            Assert.Equal(emptyOnly, method.EmptyOnly);
        }

        [Fact]
        public void DeleteOkMethod()
        {
            uint messageCount = 10;

            var method = new DeleteOkMethod(messageCount);

            Assert.Equal((uint)MethodId.QueueDeleteOk, method.Method);
            Assert.Equal(messageCount, method.MessageCount);
        }
    }
}