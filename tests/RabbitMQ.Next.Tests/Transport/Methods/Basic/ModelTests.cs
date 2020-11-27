using System.Collections.Generic;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods.Basic;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Basic
{
    public class ModelTests
    {
        [Fact]
        public void QosMethod()
        {
            var prefetchSize = (uint)12345;
            var prefetchCount = (ushort)321;
            var global = true;

            var method = new QosMethod(prefetchSize, prefetchCount, global);

            Assert.Equal((uint)MethodId.BasicQos, method.Method);
            Assert.Equal(prefetchSize, method.PrefetchSize);
            Assert.Equal(prefetchCount, method.PrefetchCount);
            Assert.Equal(global, method.Global);
        }

        [Fact]
        public void QosOkMethod()
        {
            var method = new QosOkMethod();

            Assert.Equal((uint)MethodId.BasicQosOk, method.Method);
        }

        [Fact]
        public void ConsumeMethod()
        {
            var queue = "my-queue";
            var consumerTag = "tag";
            var flags = (byte)0;
            var args = new Dictionary<string, object>();

            var method = new ConsumeMethod(queue, consumerTag, flags, args);

            Assert.Equal((uint)MethodId.BasicConsume, method.Method);
            Assert.Equal(queue, method.Queue);
            Assert.Equal(consumerTag, method.ConsumerTag);
            Assert.Equal(flags, method.Flags);
            Assert.Equal(args, method.Arguments);
        }

        [Theory]
        [InlineData(0b_00000000, false, false, false, false)]
        [InlineData(0b_00000001, true, false, false, false)]
        [InlineData(0b_00000010, false, true, false, false)]
        [InlineData(0b_00000100, false, false, true, false)]
        [InlineData(0b_00001000, false, false, false, true)]
        [InlineData(0b_00001111, true, true, true, true)]
        public void ConsumeMethodFlags(byte expected, bool noLocal, bool noAck, bool exclusive, bool noWait)
        {
            var method = new ConsumeMethod("queue", "tag", noLocal, noAck, exclusive, noWait, null);

            Assert.Equal(expected, method.Flags);
        }

        [Fact]
        public void ConsumeOkMethod()
        {
            var consumerTag = "tag";

            var method = new ConsumeOkMethod(consumerTag);

            Assert.Equal((uint)MethodId.BasicConsumeOk, method.Method);
            Assert.Equal(consumerTag, method.ConsumerTag);
        }

        [Fact]
        public void CancelMethod()
        {
            var consumerTag = "tag";
            var noWait = true;

            var method = new CancelMethod(consumerTag, noWait);

            Assert.Equal((uint)MethodId.BasicCancel, method.Method);
            Assert.Equal(consumerTag, method.ConsumerTag);
            Assert.Equal(noWait, method.NoWait);
        }

        [Fact]
        public void CancelOkMethod()
        {
            var consumerTag = "tag";

            var method = new CancelOkMethod(consumerTag);

            Assert.Equal((uint)MethodId.BasicCancelOk, method.Method);
            Assert.Equal(consumerTag, method.ConsumerTag);
        }

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

        [Fact]
        public void DeliverMethod()
        {
            var exchange = "exchange";
            var routingKey = "routing";
            var consumerTag = "tag";
            var deliveryTag = (ulong)42;
            var redelivered = true;

            var method = new DeliverMethod(exchange, routingKey, consumerTag, deliveryTag, redelivered);

            Assert.Equal((uint)MethodId.BasicDeliver, method.Method);
            Assert.Equal(exchange, method.Exchange);
            Assert.Equal(routingKey, method.RoutingKey);
            Assert.Equal(consumerTag, method.ConsumerTag);
            Assert.Equal(deliveryTag, method.DeliveryTag);
            Assert.Equal(redelivered, method.Redelivered);
        }

        [Fact]
        public void GetMethod()
        {
            var queue = "queue";
            var noAck = true;

            var method = new GetMethod(queue, noAck);

            Assert.Equal((uint)MethodId.BasicGet, method.Method);
            Assert.Equal(queue, method.Queue);
            Assert.Equal(noAck, method.NoAck);
        }

        [Fact]
        public void GetOkMethod()
        {
            var exchange = "exchange";
            var routingKey = "routing";
            var deliveryTag = (ulong)42;
            var redelivered = true;
            var messageCount = (uint)35;

            var method = new GetOkMethod(exchange, routingKey, deliveryTag, redelivered, messageCount);

            Assert.Equal((uint)MethodId.BasicGetOk, method.Method);
            Assert.Equal(exchange, method.Exchange);
            Assert.Equal(routingKey, method.RoutingKey);
            Assert.Equal(deliveryTag, method.DeliveryTag);
            Assert.Equal(redelivered, method.Redelivered);
            Assert.Equal(messageCount, method.MessageCount);
        }

        [Fact]
        public void GetEmptyMethod()
        {
            var method = new GetEmptyMethod();

            Assert.Equal((uint)MethodId.BasicGetEmpty, method.Method);
        }

        [Fact]
        public void AckMethod()
        {
            var deliveryTag = (ulong)42;
            var multiple = true;

            var method = new AckMethod(deliveryTag, multiple);

            Assert.Equal((uint)MethodId.BasicAck, method.Method);
            Assert.Equal(deliveryTag, method.DeliveryTag);
            Assert.Equal(multiple, method.Multiple);
        }

        [Fact]
        public void RecoverMethod()
        {
            var requeue = true;

            var method = new RecoverMethod(requeue);

            Assert.Equal((uint)MethodId.BasicRecover, method.Method);
            Assert.Equal(requeue, method.Requeue);
        }

        [Fact]
        public void RecoverOkMethod()
        {
            var method = new RecoverOkMethod();

            Assert.Equal((uint)MethodId.BasicRecoverOk, method.Method);
        }

        [Fact]
        public void NackMethod()
        {
            var deliveryTag = (ulong)42;
            var multiple = false;
            var requeue = true;

            var method = new NackMethod(deliveryTag, multiple, requeue);

            Assert.Equal((uint)MethodId.BasicNack, method.Method);
            Assert.Equal(deliveryTag, method.DeliveryTag);
            Assert.Equal(multiple, method.Multiple);
            Assert.Equal(requeue, method.Requeue);
        }
    }
}