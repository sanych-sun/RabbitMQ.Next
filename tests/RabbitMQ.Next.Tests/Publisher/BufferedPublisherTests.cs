using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;
using RabbitMQ.Next.Transport.Methods.Basic;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher
{
    public class BufferedPublisherTests : PublisherTestsBase
    {
        [Theory]
        [MemberData(nameof(PublishTestCases))]
        public async Task PublishAsync(
            IReadOnlyList<IMessageTransformer> transformers,
            string exchange, string routingKey, IMessageProperties properties, PublishFlags flags,
            PublishMethod expectedMethod, IMessageProperties expectedProperties)
        {
            var channel = Substitute.For<IChannel>();
            var connection = this.MockConnection();
            connection.CreateChannelAsync(Arg.Any<IEnumerable<IFrameHandler>>()).Returns(Task.FromResult(channel));

            var publisher = new BufferedPublisher(connection, this.MockSerializer(), transformers, null, 10);

            await publisher.PublishAsync("test", exchange, routingKey, properties, flags);

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

            var publisher = new BufferedPublisher(connection, this.MockSerializer(), null, null, 10);
            await publisher.DisposeAsync();

            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await publisher.PublishAsync("test"));
        }

        [Fact]
        public async Task CanDisposeMultipleTimes()
        {
            var connection = this.MockConnection();

            var publisher = new BufferedPublisher(connection, this.MockSerializer(), null, null, 10);
            await publisher.DisposeAsync();

            var ex = await Record.ExceptionAsync(async () => await publisher.DisposeAsync());

            Assert.Null(ex);
        }
    }
}