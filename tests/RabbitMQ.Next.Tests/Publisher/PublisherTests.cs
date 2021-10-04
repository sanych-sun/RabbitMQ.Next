using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Publisher.Initializers;
using RabbitMQ.Next.Serialization.Abstractions;
using RabbitMQ.Next.Tests.Mocks;
using RabbitMQ.Next.Transport.Methods.Basic;
using RabbitMQ.Next.Transport.Methods.Confirm;
using RabbitMQ.Next.Transport.Methods.Exchange;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher
{
    public class PublisherTests
    {
        // [Fact]
        // public async Task PublisherCheckExchangeExistsAsync()
        // {
        //     var mock = this.Mock();
        //
        //     var publisher = new Next.Publisher.Publisher(mock.connection, "exchange", false, this.MockSerializer(), null, null);
        //
        //     await publisher.InitializeAsync();
        //
        //     await mock.channel.Received().SendAsync<DeclareMethod, DeclareOkMethod>(new DeclareMethod("exchange"));
        // }
        //
        // [Fact]
        // public async Task PublisherThrowsIfExchangeDoesNotExistAsync()
        // {
        //     var mock = this.Mock();
        //     mock.channel.SendAsync<DeclareMethod, DeclareOkMethod>(Arg.Any<DeclareMethod>())
        //         .Throws(new ChannelClosedException("Exchange does not exists"));
        //
        //     var publisher = new Next.Publisher.Publisher(mock.connection, "exchange", false, this.MockSerializer(), null, null);
        //
        //     await Assert.ThrowsAsync<ChannelClosedException>(async () => await publisher.InitializeAsync());
        // }
        //
        // [Fact]
        // public async Task PublisherSetConfirmModeAsync()
        // {
        //     var mock = this.Mock();
        //
        //     var publisher = new Next.Publisher.Publisher(mock.connection, "exchange", true, this.MockSerializer(), null, null);
        //
        //     await publisher.InitializeAsync();
        //
        //     await mock.channel.Received().SendAsync<SelectMethod, SelectOkMethod>(Arg.Any<SelectMethod>());
        // }
        //
        // [Fact]
        // public async Task PublisherDoesNotSetConfirmModeAsync()
        // {
        //     var mock = this.Mock();
        //
        //     var publisher = new Next.Publisher.Publisher(mock.connection, "exchange", false, this.MockSerializer(), null, null);
        //
        //     await publisher.InitializeAsync();
        //
        //     await mock.channel.DidNotReceive().SendAsync<SelectMethod, SelectOkMethod>( Arg.Any<SelectMethod>());
        // }
        //
        // [Fact]
        // public async Task ShouldThrowIfConnectionClosed()
        // {
        //     var mock = this.Mock();
        //     mock.connection.State.Returns(ConnectionState.Closed);
        //
        //     var publisher = new Next.Publisher.Publisher(mock.connection, "exchange", false, this.MockSerializer(), null, null);
        //
        //     await Assert.ThrowsAsync<InvalidOperationException>(async () => await publisher.InitializeAsync());
        // }
        //
        // [Fact]
        // public async Task ShouldThrowIfDisposed()
        // {
        //     var mock = this.Mock();
        //
        //     var publisher = new Next.Publisher.Publisher(mock.connection, "exchange", false, this.MockSerializer(), null, null);
        //     await publisher.DisposeAsync();
        //
        //     await Assert.ThrowsAsync<ObjectDisposedException>(async () => await publisher.PublishAsync("test"));
        // }
        //
        // [Fact]
        // public async Task CanDisposeMultipleTimes()
        // {
        //     var mock = this.Mock();
        //
        //     var publisher = new Next.Publisher.Publisher(mock.connection, "exchange", false, this.MockSerializer(), null, null);
        //     await publisher.DisposeAsync();
        //
        //     var ex = await Record.ExceptionAsync(async () => await publisher.DisposeAsync());
        //
        //     Assert.Null(ex);
        // }
        //
        // [Fact]
        // public async Task DisposeShouldDisposeHandlers()
        // {
        //     var mock = this.Mock();
        //
        //     var returnedMessageHandler = Substitute.For<IReturnedMessageHandler>();
        //
        //     var publisher = new Next.Publisher.Publisher(mock.connection, "exchange", false, this.MockSerializer(), null, new[] {returnedMessageHandler});
        //     await publisher.DisposeAsync();
        //
        //     returnedMessageHandler.Received().Dispose();
        // }
        //
        // [Fact]
        // public async Task DisposeShouldCloseChannel()
        // {
        //     var mock = this.Mock();
        //
        //     var publisher = new Next.Publisher.Publisher(mock.connection, "exchange", false, this.MockSerializer(), null, null);
        //     await publisher.InitializeAsync();
        //     await publisher.DisposeAsync();
        //
        //     await mock.channel.Received().CloseAsync();
        // }

        // [Theory]
        // [MemberData(nameof(PublishTestCases))]
        // public async Task PublishAsync(
        //     IReadOnlyList<IMessageInitializer> transformers,
        //     string exchange, string routingKey, MessageProperties properties, PublishFlags flags,
        //     PublishMethod expectedMethod, IMessageProperties expectedProperties)
        // {
        //     var mock = this.Mock();
        //
        //     var publisher = new Next.Publisher.Publisher(mock.connection, exchange, false, this.MockSerializer(), transformers, null);
        //
        //     await publisher.PublishAsync(routingKey, "test",
        //         (state, builder) => builder.RoutingKey(state), flags);
        //
        //     await mock.channel.Received().SendAsync(
        //         expectedMethod,
        //         Arg.Is<MessageProperties>(p => new MessagePropertiesComparer().Equals(p, expectedProperties)),
        //         Arg.Any<ReadOnlySequence<byte>>()
        //     );
        // }
        //
        // [Fact]
        // public async Task PublishWaitForConfirmAck()
        // {
        //     var mock = this.Mock();
        //
        //     var publisher = new Next.Publisher.Publisher(mock.connection, "exchange", true, this.MockSerializer(), null, null);
        //
        //     var publishTask = publisher.PublishAsync("test");
        //
        //     await Task.Delay(10);
        //     Assert.False(publishTask.IsCompleted);
        //
        //     await mock.channel.EmulateMethodAsync(new AckMethod(1, false));
        //
        //     var ex = await Record.ExceptionAsync(async () => await publishTask);
        //     Assert.Null(ex);
        // }
        //
        // [Fact]
        // public async Task PublishWaitForConfirmNack()
        // {
        //     var mock = this.Mock();
        //
        //     var publisher = new Next.Publisher.Publisher(mock.connection, "exchange", true, this.MockSerializer(), null, null);
        //
        //     var publishTask = publisher.PublishAsync("test");
        //
        //     await Task.Delay(10);
        //     Assert.False(publishTask.IsCompleted);
        //
        //     await mock.channel.EmulateMethodAsync(new NackMethod(1, false, false));
        //
        //     var ex = await Record.ExceptionAsync(async () => await publishTask);
        //     Assert.NotNull(ex);
        // }
        //
        // [Fact]
        // public async Task CallReturnedMessageHandlers()
        // {
        //     var mock = this.Mock();
        //     var returnedMessagesHandler = Substitute.For<IReturnedMessageHandler>();
        //     returnedMessagesHandler.TryHandleAsync(Arg.Any<ReturnedMessage>(), Arg.Any<IMessageProperties>(), Arg.Any<Content>())
        //         .Returns(new ValueTask<bool>(true));
        //
        //     var publisher = new Next.Publisher.Publisher(mock.connection, "exchange", false, this.MockSerializer(), null, new [] { returnedMessagesHandler } );
        //     await publisher.PublishAsync("test");
        //
        //     var props = Substitute.For<IMessageProperties>();
        //     await mock.channel.EmulateMethodAsync(new ReturnMethod("exchange", null, 301, "test"), props, ReadOnlySequence<byte>.Empty);
        //
        //     await returnedMessagesHandler.Received().TryHandleAsync(Arg.Any<ReturnedMessage>(), props, Arg.Any<Content>());
        // }
        //
        // [Fact]
        // public async Task CallMultipleReturnedMessageHandlers()
        // {
        //     var mock = this.Mock();
        //     var returnedMessagesHandler1 = Substitute.For<IReturnedMessageHandler>();
        //     returnedMessagesHandler1.TryHandleAsync(Arg.Any<ReturnedMessage>(), Arg.Any<IMessageProperties>(), Arg.Any<Content>())
        //         .Returns(new ValueTask<bool>(false));
        //     var returnedMessagesHandler2 = Substitute.For<IReturnedMessageHandler>();
        //     returnedMessagesHandler2.TryHandleAsync(Arg.Any<ReturnedMessage>(), Arg.Any<IMessageProperties>(), Arg.Any<Content>())
        //         .Returns(new ValueTask<bool>(true));
        //     var returnedMessagesHandler3 = Substitute.For<IReturnedMessageHandler>();
        //     returnedMessagesHandler3.TryHandleAsync(Arg.Any<ReturnedMessage>(), Arg.Any<IMessageProperties>(), Arg.Any<Content>())
        //         .Returns(new ValueTask<bool>(true));
        //
        //     var publisher = new Next.Publisher.Publisher(mock.connection, "exchange", false, this.MockSerializer(), null,
        //         new [] { returnedMessagesHandler1, returnedMessagesHandler2, returnedMessagesHandler3 } );
        //     await publisher.PublishAsync("test");
        //
        //     var props = Substitute.For<IMessageProperties>();
        //     await mock.channel.EmulateMethodAsync(new ReturnMethod("exchange", null, 301, "test"), props, ReadOnlySequence<byte>.Empty);
        //
        //     await returnedMessagesHandler1.Received().TryHandleAsync(Arg.Any<ReturnedMessage>(), props, Arg.Any<Content>());
        //     await returnedMessagesHandler2.Received().TryHandleAsync(Arg.Any<ReturnedMessage>(), props, Arg.Any<Content>());
        //     await returnedMessagesHandler3.DidNotReceive().TryHandleAsync(Arg.Any<ReturnedMessage>(), props, Arg.Any<Content>());
        // }
        //
        public static IEnumerable<object[]> PublishTestCases()
        {
            yield return new object[]
            {
                null,
                "myExchange", "key", new MessageProperties(), PublishFlags.None,
                new PublishMethod("myExchange", "key", 0),
                new MessageProperties()
            };

            yield return new object[]
            {
                null,
                "myExchange", "key", new MessageProperties(), PublishFlags.Immediate,
                new PublishMethod("myExchange", "key", (byte)PublishFlags.Immediate),
                new MessageProperties()
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
                null,
                "myExchange", "key", new MessageProperties { Priority = 1, Type = "test"}, PublishFlags.None,
                new PublishMethod("myExchange", "key", 0),
                new MessageProperties { Priority = 1, Type = "test"}
            };

            yield return new object[]
            {
                new IMessageInitializer[] { new UserIdInitializer("testUser")},
                "exchange", "key", new MessageProperties { Priority = 1, Type = "test"}, PublishFlags.None,
                new PublishMethod("exchange", "key", 0),
                new MessageProperties { Priority = 1, Type = "test", UserId = "testUser"}
            };
        }

        protected ISerializer MockSerializer()
        {
            var serializer = Substitute.For<ISerializer>();
            serializer
                .When(x => x.Serialize(Arg.Any<string>(), Arg.Any<IBufferWriter<byte>>()))
                .Do(x => x.ArgAt<IBufferWriter<byte>>(1).Write(new byte[] { 0x01, 0x02}));

            return serializer;
        }

        private (IConnection connection, IChannel channel) Mock()
        {
            var channel = Substitute.For<IChannel>();

            var connection = Substitute.For<IConnection>();
            connection.State.Returns(ConnectionState.Open);
            connection.OpenChannelAsync(Arg.Any<IReadOnlyList<IFrameHandler>>())
                .Returns(args => Task.FromResult(channel));

            return (connection, channel);
        }
    }
}