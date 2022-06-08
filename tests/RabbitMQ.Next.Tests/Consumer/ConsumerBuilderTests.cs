using System;
using NSubstitute;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Consumer;
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
            var ackFactory = Substitute.For<Func<IChannel, IAcknowledgement>>();

            ((IConsumerBuilder) consumerBuilder).SetAcknowledgement(ackFactory);

            Assert.Equal(ackFactory, consumerBuilder.AcknowledgementFactory);
        }

        [Fact]
        public void SetAcknowledgerThrowsOnNull()
        {
            var consumerBuilder = new ConsumerBuilder();

            Assert.Throws<ArgumentNullException>(() => ((IConsumerBuilder) consumerBuilder).SetAcknowledgement(null));
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
        public void MessageHandler()
        {
            var consumerBuilder = new ConsumerBuilder();
            var handler1 = Substitute.For<IDeliveredMessageHandler>();
            var handler2 = Substitute.For<IDeliveredMessageHandler>();

            ((IConsumerBuilder) consumerBuilder).MessageHandler(handler1);
            ((IConsumerBuilder) consumerBuilder).MessageHandler(handler2);

            Assert.Contains(handler1, consumerBuilder.Handlers);
            Assert.Contains(handler2, consumerBuilder.Handlers);
        }

        [Fact]
        public void MessageHandlerThrowsOnNull()
        {
            var consumerBuilder = new ConsumerBuilder();

            Assert.Throws<ArgumentNullException>(() => ((IConsumerBuilder)consumerBuilder).MessageHandler(null));
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