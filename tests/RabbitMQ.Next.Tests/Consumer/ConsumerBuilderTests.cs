using System;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Consumer;
using RabbitMQ.Next.Consumer.Abstractions;
using RabbitMQ.Next.Serialization.Abstractions;
using Xunit;

namespace RabbitMQ.Next.Tests.Consumer
{
    public class ConsumerBuilderTests
    {
        [Fact]
        public void UseFormatter()
        {
            var consumerBuilder = new ConsumerBuilder();
            var formatter1 = Substitute.For<ITypeFormatter>();
            var formatter2 = Substitute.For<ITypeFormatter>();

            ((IConsumerBuilder) consumerBuilder).UseFormatter(formatter1);
            ((IConsumerBuilder) consumerBuilder).UseFormatter(formatter2);

            Assert.Contains(formatter1, consumerBuilder.Formatters);
            Assert.Contains(formatter2, consumerBuilder.Formatters);
        }

        [Fact]
        public void UseFormatterThrowsOnNull()
        {
            var consumerBuilder = new ConsumerBuilder();

            Assert.Throws<ArgumentNullException>(() => ((IConsumerBuilder) consumerBuilder).UseFormatter(null));
        }
        
        [Fact]
        public void UseFormatterSource()
        {
            var consumerBuilder = new ConsumerBuilder();
            var formatterSource1 = Substitute.For<IFormatterSource>();
            var formatterSource2 = Substitute.For<IFormatterSource>();

            ((IConsumerBuilder) consumerBuilder).UseFormatterSource(formatterSource1);
            ((IConsumerBuilder) consumerBuilder).UseFormatterSource(formatterSource2);

            Assert.Contains(formatterSource1, consumerBuilder.FormatterSources);
            Assert.Contains(formatterSource2, consumerBuilder.FormatterSources);
        }

        [Fact]
        public void UseFormatterSourceThrowsOnNull()
        {
            var consumerBuilder = new ConsumerBuilder();

            Assert.Throws<ArgumentNullException>(() => ((IConsumerBuilder) consumerBuilder).UseFormatterSource(null));
        }

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
            var handler1 = Substitute.For<Func<DeliveredMessage, IMessageProperties, Content, ValueTask<bool>>>();
            var handler2 = Substitute.For<Func<DeliveredMessage, IMessageProperties, Content, ValueTask<bool>>>();

            ((IConsumerBuilder) consumerBuilder).AddMessageHandler(handler1);
            ((IConsumerBuilder) consumerBuilder).AddMessageHandler(handler2);

            Assert.Contains(handler1, consumerBuilder.Handlers);
            Assert.Contains(handler2, consumerBuilder.Handlers);
        }

        [Fact]
        public void AddMessageHandlerThrowsOnNull()
        {
            var consumerBuilder = new ConsumerBuilder();

            Assert.Throws<ArgumentNullException>(() => ((IConsumerBuilder) consumerBuilder).AddMessageHandler(null));
        }
    }
}