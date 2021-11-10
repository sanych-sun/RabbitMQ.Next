using System;
using System.Linq;
using NSubstitute;
using RabbitMQ.Next.Consumer;
using RabbitMQ.Next.Consumer.Abstractions;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Serialization.Abstractions;
using Xunit;

namespace RabbitMQ.Next.Tests.Consumer
{
    public class ConsumerBuilderTests
    {
        [Fact]
        public void PrefetchSize()
        {
            var serializerFactory = Substitute.For<ISerializerFactory>();
            var consumerBuilder = new ConsumerBuilder(serializerFactory);
            uint size = 12345;

            ((IConsumerBuilder) consumerBuilder).PrefetchSize(size);

            Assert.Equal(size, consumerBuilder.PrefetchSize);
        }
        
        [Fact]
        public void PrefetchCount()
        {
            var serializerFactory = Substitute.For<ISerializerFactory>();
            var consumerBuilder = new ConsumerBuilder(serializerFactory);
            ushort count = 42;

            ((IConsumerBuilder) consumerBuilder).PrefetchCount(count);

            Assert.Equal(count, consumerBuilder.PrefetchCount);
        }

        [Fact]
        public void SetAcknowledger()
        {
            var serializerFactory = Substitute.For<ISerializerFactory>();
            var consumerBuilder = new ConsumerBuilder(serializerFactory);
            var ackFactory = Substitute.For<Func<IAcknowledgement, IAcknowledger>>();

            ((IConsumerBuilder) consumerBuilder).SetAcknowledger(ackFactory);

            Assert.Equal(ackFactory, consumerBuilder.AcknowledgerFactory);
        }

        [Fact]
        public void SetAcknowledgerThrowsOnNull()
        {
            var serializerFactory = Substitute.For<ISerializerFactory>();
            var consumerBuilder = new ConsumerBuilder(serializerFactory);

            Assert.Throws<ArgumentNullException>(() => ((IConsumerBuilder) consumerBuilder).SetAcknowledger(null));
        }

        [Fact]
        public void OnUnprocessedMessage()
        {
            var serializerFactory = Substitute.For<ISerializerFactory>();
            var consumerBuilder = new ConsumerBuilder(serializerFactory);
            var val = UnprocessedMessageMode.Drop;

            ((IConsumerBuilder) consumerBuilder).OnUnprocessedMessage(val);

            Assert.Equal(val, consumerBuilder.OnUnprocessedMessage);
        }

        [Fact]
        public void OnPoisonMessage()
        {
            var serializerFactory = Substitute.For<ISerializerFactory>();
            var consumerBuilder = new ConsumerBuilder(serializerFactory);
            var val = UnprocessedMessageMode.Drop;

            ((IConsumerBuilder) consumerBuilder).OnPoisonMessage(val);

            Assert.Equal(val, consumerBuilder.OnPoisonMessage);
        }

        [Fact]
        public void AddMessageHandler()
        {
            var serializerFactory = Substitute.For<ISerializerFactory>();
            var consumerBuilder = new ConsumerBuilder(serializerFactory);
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
            var serializerFactory = Substitute.For<ISerializerFactory>();
            var consumerBuilder = new ConsumerBuilder(serializerFactory);

            Assert.Throws<ArgumentNullException>(() => ((IConsumerBuilder) consumerBuilder).AddMessageHandler(null));
        }

        [Fact]
        public void RegisterSerializerCallFactory()
        {
            var serializerFactory = Substitute.For<ISerializerFactory>();
            var builder = new ConsumerBuilder(serializerFactory);
            var serializer = Substitute.For<ISerializer>();

            var types = new[] { "type" };
            builder.UseSerializer(serializer, types);

            serializerFactory.Received().RegisterSerializer(serializer, types);
        }

        [Fact]
        public void ReturnsFactory()
        {
            var serializerFactory = Substitute.For<ISerializerFactory>();

            var builder = new ConsumerBuilder(serializerFactory);
            Assert.Equal(serializerFactory, builder.SerializerFactory);
        }

        [Fact]
        public void DefaultBindToQueue()
        {
            var queueName = "q1";
            var serializerFactory = Substitute.For<ISerializerFactory>();
            var builder = new ConsumerBuilder(serializerFactory);

            ((IConsumerBuilder)builder).BindToQueue(queueName, null);

            Assert.True(builder.Queues.Any(x => x.Queue == queueName));
        }

        [Fact]
        public void CanBindToQueue()
        {
            var queueName = "q1";
            var serializerFactory = Substitute.For<ISerializerFactory>();
            var consumerBuilder = Substitute.For<Action<IQueueConsumerBuilder>>();
            var builder = new ConsumerBuilder(serializerFactory);

            ((IConsumerBuilder)builder).BindToQueue(queueName, consumerBuilder);

            consumerBuilder.Received();
            Assert.True(builder.Queues.Any(x => x.Queue == queueName));
        }
    }
}