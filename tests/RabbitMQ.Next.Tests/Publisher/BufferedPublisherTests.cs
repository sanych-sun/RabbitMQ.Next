using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;
using RabbitMQ.Next.Transport.Methods.Basic;
using RabbitMQ.Next.Transport.Methods.Exchange;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher
{
    public class BufferedPublisherTests : PublisherTestsBase
    {
        [Fact]
        public async Task PublishCheckExchangeExistsAsync()
        {
            var channel = this.MockChannel();
            var connection = this.MockConnection();
            connection.CreateChannelAsync(Arg.Any<IEnumerable<IMethodHandler>>()).Returns(Task.FromResult(channel));

            var publisher = new BufferedPublisher(connection, "exchange", this.MockSerializer(), null, null, 10);

            await publisher.PublishAsync("test");

            await channel.Received().SendAsync<DeclareMethod, DeclareOkMethod>(new DeclareMethod("exchange"));
        }

        [Theory]
        [MemberData(nameof(PublishTestCases))]
        public async Task PublishAsync(
            IReadOnlyList<IMessageTransformer> transformers,
            string exchange, string routingKey, IMessageProperties properties, PublishFlags flags,
            PublishMethod expectedMethod, IMessageProperties expectedProperties)
        {
            var channel = Substitute.For<IChannel>();
            var connection = this.MockConnection();
            connection.CreateChannelAsync(Arg.Any<IEnumerable<IMethodHandler>>()).Returns(Task.FromResult(channel));

            var publisher = new BufferedPublisher(connection, exchange, this.MockSerializer(), transformers, null, 10);

            await publisher.PublishAsync("test", routingKey, properties, flags);

            await publisher.CompleteAsync();

            await channel.Received().SendAsync(
                expectedMethod,
                Arg.Is<IMessageProperties>(p => new MessagePropertiesComparer().Equals(p, expectedProperties)),
                Arg.Any<ReadOnlySequence<byte>>()
            );
        }

        [Fact]
        public async Task ShouldThrowIfDisposed()
        {
            var connection = this.MockConnection();

            var publisher = new BufferedPublisher(connection, "exchange", this.MockSerializer(), null, null, 10);
            await publisher.DisposeAsync();

            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await publisher.PublishAsync("test"));
        }

        [Fact]
        public async Task CanDisposeMultipleTimes()
        {
            var connection = this.MockConnection();

            var publisher = new BufferedPublisher(connection, "exchange", this.MockSerializer(), null, null, 10);
            await publisher.DisposeAsync();

            var ex = await Record.ExceptionAsync(async () => await publisher.DisposeAsync());

            Assert.Null(ex);
        }
    }
}