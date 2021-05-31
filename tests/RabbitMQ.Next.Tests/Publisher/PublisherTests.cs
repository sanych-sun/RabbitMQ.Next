using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Buffers;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;
using RabbitMQ.Next.Publisher.Transformers;
using RabbitMQ.Next.Serialization.Abstractions;
using RabbitMQ.Next.Transport.Buffers;
using RabbitMQ.Next.Transport.Methods.Basic;
using RabbitMQ.Next.Transport.Methods.Exchange;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher
{
    public class PublisherTests
    {
        [Fact]
        public async Task PublisherCheckExchangeExistsAsync()
        {
            var mock = this.Mock();

            var publisher = new Next.Publisher.Publisher(mock.connection, "exchange", false, this.MockSerializer(), null, null);

            await publisher.PublishAsync("test");

            await mock.sync.Received().SendAsync(new DeclareMethod("exchange"));
        }

        [Fact]
        public async Task ShouldThrowIfConnectionClosed()
        {
            var mock = this.Mock();
            mock.connection.State.Returns(ConnectionState.Closed);

            var publisher = new Next.Publisher.Publisher(mock.connection, "exchange", false, this.MockSerializer(), null, null);

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await publisher.PublishAsync("test"));
        }

        [Fact]
        public async Task ShouldThrowIfDisposed()
        {
            var mock = this.Mock();

            var publisher = new Next.Publisher.Publisher(mock.connection, "exchange", false, this.MockSerializer(), null, null);
            await publisher.DisposeAsync();

            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await publisher.PublishAsync("test"));
        }

        [Fact]
        public async Task CanDisposeMultipleTimes()
        {
            var mock = this.Mock();

            var publisher = new Next.Publisher.Publisher(mock.connection, "exchange", false, this.MockSerializer(), null, null);
            await publisher.DisposeAsync();

            var ex = await Record.ExceptionAsync(async () => await publisher.DisposeAsync());

            Assert.Null(ex);
        }

        [Fact]
        public async Task CompleteShouldDispose()
        {
            var mock = this.Mock();

            var publisher = new Next.Publisher.Publisher(mock.connection, "exchange", false, this.MockSerializer(), null, null);
            await publisher.CompleteAsync();

            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await publisher.PublishAsync("test"));
        }

        [Fact]
        public async Task DisposeShouldDisposeHandlers()
        {
            var mock = this.Mock();

            var returnedMessageHandler = Substitute.For<IReturnedMessageHandler>();

            var publisher = new Next.Publisher.Publisher(mock.connection, "exchange", false, this.MockSerializer(), null, new[] {returnedMessageHandler});
            await publisher.DisposeAsync();

            returnedMessageHandler.Received().Dispose();
        }

        [Fact]
        public async Task DisposeShouldCloseChannel()
        {
            var mock = this.Mock();

            var publisher = new Next.Publisher.Publisher(mock.connection, "exchange", false, this.MockSerializer(), null, null);
            await publisher.PublishAsync("test", "route");
            await publisher.DisposeAsync();

            Assert.True(mock.channel.IsClosed);
        }

        [Theory]
        [MemberData(nameof(PublishTestCases))]
        public async Task PublishAsync(
            IReadOnlyList<IMessageTransformer> transformers,
            string exchange, string routingKey, IMessageProperties properties, PublishFlags flags,
            PublishMethod expectedMethod, IMessageProperties expectedProperties)
        {
            var mock = this.Mock();

            var publisher = new Next.Publisher.Publisher(mock.connection, exchange, false, this.MockSerializer(), transformers, null);

            await publisher.PublishAsync("test", routingKey, properties, flags);

            await mock.sync.Received().SendAsync(
                expectedMethod,
                Arg.Is<IMessageProperties>(p => new MessagePropertiesComparer().Equals(p, expectedProperties)),
                Arg.Any<ReadOnlySequence<byte>>()
            );
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
                "myExchange", "key", new MessagePropertiesMock { ApplicationId = "test"}, PublishFlags.None,
                new PublishMethod("myExchange", "key", 0),
                new MessagePropertiesMock { ApplicationId = "test"}
            };

            yield return new object[]
            {
                null,
                "myExchange", "key", new MessagePropertiesMock { Priority = 1, Type = "test"}, PublishFlags.None,
                new PublishMethod("myExchange", "key", 0),
                new MessagePropertiesMock { Priority = 1, Type = "test"}
            };

            yield return new object[]
            {
                new IMessageTransformer[] { new UserIdTransformer("testUser")},
                "exchange", "key", new MessagePropertiesMock { Priority = 1, Type = "test"}, PublishFlags.None,
                new PublishMethod("exchange", "key", 0),
                new MessagePropertiesMock { Priority = 1, Type = "test", UserId = "testUser"}
            };
        }

        protected ISerializer MockSerializer()
        {
            var serializer = Substitute.For<ISerializer>();
            serializer
                .When(x => x.Serialize(Arg.Any<string>(), Arg.Any<IBufferWriter>()))
                .Do(x => x.ArgAt<IBufferWriter>(1).Write(new byte[] { 0x01, 0x02}));

            return serializer;
        }

        private (IConnection connection, ChannelMock channel, ISynchronizedChannel sync) Mock()
        {
            var syncChannel = Substitute.For<ISynchronizedChannel>();
            syncChannel.WaitAsync<DeclareOkMethod>().Returns(new DeclareOkMethod());

            var channel = new ChannelMock(syncChannel);

            var connection = Substitute.For<IConnection>();
            connection.BufferPool.Returns(new BufferPool(1024));
            connection.State.Returns(ConnectionState.Open);
            connection.CreateChannelAsync(Arg.Any<IEnumerable<IMethodHandler>>()).Returns(Task.FromResult((IChannel)channel));

            return (connection, channel, syncChannel);
        }

        public class ChannelMock : IChannel
        {
            private readonly ISynchronizedChannel channel;

            public ChannelMock(ISynchronizedChannel channel)
            {
                this.Completion = new TaskCompletionSource<bool>().Task;
                this.channel = channel;
            }

            public bool IsClosed { get; private set; }

            public Task Completion { get; }

            public Task UseChannel(Func<ISynchronizedChannel, Task> fn, CancellationToken cancellation = default)
                => fn(this.channel);

            public Task UseChannel<TState>(TState state, Func<ISynchronizedChannel, TState, Task> fn, CancellationToken cancellation = default)
                => fn(this.channel, state);

            public Task<TResult> UseChannel<TResult>(Func<ISynchronizedChannel, Task<TResult>> fn, CancellationToken cancellation = default)
                => fn(this.channel);

            public Task<TResult> UseChannel<TState, TResult>(TState state, Func<ISynchronizedChannel, TState, Task<TResult>> fn, CancellationToken cancellation = default)
                => fn(this.channel, state);

            public Task CloseAsync()
            {
                this.IsClosed = true;
                return Task.CompletedTask;
            }

            public Task CloseAsync(ushort statusCode, string description, MethodId failedMethodId)
            {
                this.IsClosed = true;
                return Task.CompletedTask;
            }
        }
    }
}