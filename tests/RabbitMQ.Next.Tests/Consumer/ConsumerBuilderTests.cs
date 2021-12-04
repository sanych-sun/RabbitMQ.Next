using System;
using NSubstitute;
using RabbitMQ.Next.Consumer;
using RabbitMQ.Next.Consumer.Abstractions;
using RabbitMQ.Next.Serialization.Abstractions;
using Xunit;

namespace RabbitMQ.Next.Tests.Consumer
{
    public class ConsumerBuilderTests
    {
        [Fact]
        public void PrefetchSize()
        {
            var consumerBuilder = new ConsumerBuilder();
            uint size = 12345;

            ((IConsumerBuilder) consumerBuilder).PrefetchSize(size);

            Assert.Equal(size, consumerBuilder.PrefetchSize);
        }
        
        [Fact]
        public void PrefetchCount()
        {
            var consumerBuilder = new ConsumerBuilder();
            ushort count = 42;

            ((IConsumerBuilder) consumerBuilder).PrefetchCount(count);

            Assert.Equal(count, consumerBuilder.PrefetchCount);
        }

        [Fact]
        public void SetAcknowledger()
        {
            var consumerBuilder = new ConsumerBuilder();
            var ackFactory = Substitute.For<Func<IAcknowledgement, IAcknowledger>>();

            ((IConsumerBuilder) consumerBuilder).SetAcknowledger(ackFactory);

            Assert.Equal(ackFactory, consumerBuilder.AcknowledgerFactory);
        }

        [Fact]
        public void SetAcknowledgerThrowsOnNull()
        {
            var consumerBuilder = new ConsumerBuilder();

            Assert.Throws<ArgumentNullException>(() => ((IConsumerBuilder) consumerBuilder).SetAcknowledger(null));
        }

        [Fact]
        public void OnUnprocessedMessage()
        {
            var consumerBuilder = new ConsumerBuilder();
            var val = UnprocessedMessageMode.Drop;

            ((IConsumerBuilder) consumerBuilder).OnUnprocessedMessage(val);

            Assert.Equal(val, consumerBuilder.OnUnprocessedMessage);
        }

        [Fact]
        public void OnPoisonMessage()
        {
            var consumerBuilder = new ConsumerBuilder();
            var val = UnprocessedMessageMode.Drop;

            ((IConsumerBuilder) consumerBuilder).OnPoisonMessage(val);

            Assert.Equal(val, consumerBuilder.OnPoisonMessage);
        }

        [Fact]
        public void AddMessageHandler()
        {
            var consumerBuilder = new ConsumerBuilder();
            var handler1 = Substitute.For<IDeliveredMessageHandler>();
            var handler2 = Substitute.For<IDeliveredMessageHandler>();

            ((IConsumerBuilder) consumerBuilder).AddMessageHandler(handler1);
            ((IConsumerBuilder) consumerBuilder).AddMessageHandler(handler2);

            Assert.Contains(handler1, consumerBuilder.Handlers);
            Assert.Contains(handler2, consumerBuilder.Handlers);
        }

        [Fact]
        public void AddMessageHandlerThrowsOnNull()
        {
            var consumerBuilder = new ConsumerBuilder();

            Assert.Throws<ArgumentNullException>(() => consumerBuilder.AddMessageHandler(null));
        }

        [Fact]
        public void UseSerializer()
        {
            var serializer = Substitute.For<ISerializer>();
            var contentType = "application/json";
            var isDefault = false;

            var builder = new ConsumerBuilder();

            builder.UseSerializer(serializer, contentType, isDefault);

            Assert.Contains(builder.Serializers, s => s.Serializer == serializer && s.ContentType == contentType && s.Default == isDefault);
        }

        [Fact]
        public void UseSerializerThrowsOnNull()
        {
            var builder = new ConsumerBuilder();
            Assert.Throws<ArgumentNullException>(() => ((IConsumerBuilder) builder).UseSerializer(null));
        }

        [Fact]
        public void DefaultBindToQueue()
        {
            var queueName = "q1";
            var builder = new ConsumerBuilder();

            ((IConsumerBuilder)builder).BindToQueue(queueName, null);

            Assert.Contains(builder.Queues, x => x.Queue == queueName);
        }

        [Fact]
        public void CanBindToQueue()
        {
            var queueName = "q1";
            var consumerBuilder = Substitute.For<Action<IQueueConsumerBuilder>>();
            var builder = new ConsumerBuilder();

            ((IConsumerBuilder)builder).BindToQueue(queueName, consumerBuilder);

            consumerBuilder.Received();
            Assert.Contains(builder.Queues, x => x.Queue == queueName);
        }
    }
}