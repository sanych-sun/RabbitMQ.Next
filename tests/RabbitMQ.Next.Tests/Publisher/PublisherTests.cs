using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Buffers;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;
using RabbitMQ.Next.Publisher.Transformers;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Transport.Buffers;
using RabbitMQ.Next.Transport.Methods.Basic;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher
{
    public class PublisherTests
    {
        [Theory]
        [MemberData(nameof(PublishTestCases))]
        public async Task PublishAsync(
            IReadOnlyList<IMessageTransformer> transformers, 
            string exchange, string routingKey, IMessageProperties properties, PublishFlags flags,
            PublishMethod expectedMethod, IMessageProperties expectedProperties)
        {
            var channel = Substitute.For<IChannel>();
            var connection = Substitute.For<IConnection>();
            connection.BufferPool.Returns(new BufferPool(1024));
            connection.State.Returns(ConnectionState.Open);
            connection.CreateChannelAsync(Arg.Any<IEnumerable<IFrameHandler>>()).Returns(Task.FromResult(channel));

            var serializer = Substitute.For<ISerializer>();
            serializer
                .When(x => x.Serialize(Arg.Any<string>(), Arg.Any<IBufferWriter>()))
                .Do(x => x.ArgAt<IBufferWriter>(1).Write(new byte[] { 0x01, 0x02}));

            var publisher = new Next.Publisher.Publisher(connection, serializer, transformers, null);

            await publisher.PublishAsync("test", exchange, routingKey, properties, flags);

            await channel.Received().SendAsync(
                expectedMethod,
                Arg.Is<IMessageProperties>(p => new MessagePropertiesComparer().Equals(p, expectedProperties)),
                Arg.Any<ReadOnlySequence<byte>>()
            );
        }

        [Fact]
        public async Task ShouldThrowIfConnectionClosed()
        {
            var connection = Substitute.For<IConnection>();
            connection.State.Returns(ConnectionState.Closed);
            var serializer = Substitute.For<ISerializer>();

            var publisher = new Next.Publisher.Publisher(connection, serializer, null, null);

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await publisher.PublishAsync("test"));
        }

        [Fact]
        public async Task ShouldThrowIfDisposed()
        {
            var connection = Substitute.For<IConnection>();
            connection.State.Returns(ConnectionState.Open);
            var serializer = Substitute.For<ISerializer>();

            var publisher = new Next.Publisher.Publisher(connection, serializer, null, null);
            await publisher.DisposeAsync();

            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await publisher.PublishAsync("test"));
        }

        [Fact]
        public async Task CanDisposeMultipleTimes()
        {
            var connection = Substitute.For<IConnection>();
            connection.State.Returns(ConnectionState.Open);
            var serializer = Substitute.For<ISerializer>();

            var publisher = new Next.Publisher.Publisher(connection, serializer, null, null);
            await publisher.DisposeAsync();

            var ex = await Record.ExceptionAsync(async () => await publisher.DisposeAsync());

            Assert.Null(ex);
        }

        [Fact]
        public async Task CompleteShouldDispose()
        {
            var connection = Substitute.For<IConnection>();
            connection.State.Returns(ConnectionState.Open);
            var serializer = Substitute.For<ISerializer>();

            var publisher = new Next.Publisher.Publisher(connection, serializer, null, null);
            await publisher.CompleteAsync();

            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await publisher.PublishAsync("test"));
        }

        public static IEnumerable<object[]> PublishTestCases()
        {
            yield return new object[]
            {
                null,
                "myExchange", "key", null, PublishFlags.None,
                new PublishMethod("myExchange", "key", 0),
                null
            };

            yield return new object[]
            {
                null,
                "myExchange", "key", null, PublishFlags.Immediate,
                new PublishMethod("myExchange", "key", (byte)PublishFlags.Immediate),
                null
            };

            yield return new object[]
            {
                null,
                "myExchange", "key", new MessageProperties { ApplicationId = "test"}, PublishFlags.None,
                new PublishMethod("myExchange", "key", 0),
                new MessageProperties { ApplicationId = "test"}
            };

            yield return new object[]
            {
                new IMessageTransformer[] { new ExchangeTransformer("default")},
                "", "key", null, PublishFlags.None,
                new PublishMethod("default", "key", 0),
                new MessageProperties()
            };

            yield return new object[]
            {
                null,
                "myExchange", "key", new MessageProperties { Priority = 1, Type = "test"}, PublishFlags.None,
                new PublishMethod("myExchange", "key", 0),
                new MessageProperties { Priority = 1, Type = "test"}
            };

            yield return new object[]
            {
                new IMessageTransformer[] { new UserIdTransformer("testUser")},
                "exchange", "key", new MessageProperties { Priority = 1, Type = "test"}, PublishFlags.None,
                new PublishMethod("exchange", "key", 0),
                new MessageProperties { Priority = 1, Type = "test", UserId = "testUser"}
            };
        }
    }
}