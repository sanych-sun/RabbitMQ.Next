using NSubstitute;
using RabbitMQ.Next.TopologyBuilder.Abstractions;
using Xunit;

namespace RabbitMQ.Next.Tests.TopologyBuilder
{
    public class QueueBuilderExtensionsTests
    {
        [Fact]
        public void WithMessageTtl()
        {
            var ttl = 12345;
            var builder = Substitute.For<IQueueBuilder>();

            builder.WithMessageTtl(ttl);

            builder.Received().SetArgument("x-message-ttl", ttl);
        }

        [Fact]
        public void WithDeadLetterExchange()
        {
            var exchange = "test";
            var builder = Substitute.For<IQueueBuilder>();

            builder.WithDeadLetterExchange(exchange);

            builder.Received().SetArgument("x-dead-letter-exchange", exchange);
            builder.DidNotReceive().SetArgument("x-dead-letter-routing-key", Arg.Any<string>());
        }

        [Fact]
        public void WithDeadLetterExchangeRouting()
        {
            var exchange = "test";
            var routeKey = "route-key";
            var builder = Substitute.For<IQueueBuilder>();

            builder.WithDeadLetterExchange(exchange, routeKey);

            builder.Received().SetArgument("x-dead-letter-exchange", exchange);
            builder.Received().SetArgument("x-dead-letter-routing-key", routeKey);
        }

        [Fact]
        public void WithMaxLength()
        {
            var maxLength = 42;
            var builder = Substitute.For<IQueueBuilder>();

            builder.WithMaxLength(maxLength);

            builder.Received().SetArgument("x-max-length", maxLength);
        }

        [Fact]
        public void WithMaxSize()
        {
            var maxSize = 1024;
            var builder = Substitute.For<IQueueBuilder>();

            builder.WithMaxSize(maxSize);

            builder.Received().SetArgument("x-max-length-bytes", maxSize);
        }

        [Fact]
        public void WithDropOnOverflow()
        {
            var builder = Substitute.For<IQueueBuilder>();

            builder.WithDropOnOverflow();

            builder.Received().SetArgument("x-overflow", "drop-head");
        }

        [Fact]
        public void WithRejectOnOverflow()
        {
            var builder = Substitute.For<IQueueBuilder>();

            builder.WithRejectOnOverflow();

            builder.Received().SetArgument("x-overflow", "reject-publish");
        }

        [Fact]
        public void WithMaxPriority()
        {
            byte maxPriority = 42;
            var builder = Substitute.For<IQueueBuilder>();

            builder.WithMaxPriority(maxPriority);

            builder.Received().SetArgument("x-max-priority", maxPriority);
        }

        [Fact]
        public void AsLazy()
        {
            var builder = Substitute.For<IQueueBuilder>();

            builder.AsLazy();

            builder.Received().SetArgument("x-queue-mode", "lazy");
        }
    }
}