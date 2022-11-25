using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Publisher.Initializers;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Tests.Mocks;
using RabbitMQ.Next.Transport.Methods.Confirm;
using RabbitMQ.Next.Transport.Methods.Exchange;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher;

public class PublisherTests
{
    [Fact]
    public async Task PublisherCheckExchangeExistsAsync()
    {
        var mock = this.Mock();

        var publisher = new Next.Publisher.Publisher(mock.connection, this.PoolStub, this.MockSerializer(), "exchange", false, null, null);

        await publisher.InitializeAsync();

        await mock.channel.Received().SendAsync<DeclareMethod, DeclareOkMethod>(new DeclareMethod("exchange"));
    }

    [Fact]
    public async Task PublisherThrowsIfExchangeDoesNotExistAsync()
    {
        var mock = this.Mock();
        mock.channel.SendAsync<DeclareMethod, DeclareOkMethod>(Arg.Any<DeclareMethod>())
            .Throws(new ChannelClosedException("Exchange does not exists"));

        var publisher = new Next.Publisher.Publisher(mock.connection, this.PoolStub, this.MockSerializer(), "exchange", false, null, null);

        await Assert.ThrowsAsync<ChannelClosedException>(async () => await publisher.InitializeAsync());
    }

    [Fact]
    public async Task PublisherSetConfirmModeAsync()
    {
        var mock = this.Mock();

        var publisher = new Next.Publisher.Publisher(mock.connection, this.PoolStub, this.MockSerializer(), "exchange", true, null, null);

        await publisher.InitializeAsync();

        await mock.channel.Received().SendAsync<SelectMethod, SelectOkMethod>(Arg.Any<SelectMethod>());
    }

    [Fact]
    public async Task PublisherDoesNotSetConfirmModeAsync()
    {
        var mock = this.Mock();

        var publisher = new Next.Publisher.Publisher(mock.connection, this.PoolStub, this.MockSerializer(), "exchange", false, null, null);

        await publisher.InitializeAsync();

        await mock.channel.DidNotReceive().SendAsync<SelectMethod, SelectOkMethod>( Arg.Any<SelectMethod>());
    }

    [Fact]
    public async Task ShouldThrowIfConnectionClosed()
    {
        var mock = this.Mock();
        mock.connection.State.Returns(ConnectionState.Closed);

        var publisher = new Next.Publisher.Publisher(mock.connection, this.PoolStub, this.MockSerializer(), "exchange", false, null, null);

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await publisher.InitializeAsync());
    }

    [Fact]
    public async Task ShouldThrowIfDisposed()
    {
        var mock = this.Mock();

        var publisher = new Next.Publisher.Publisher(mock.connection, this.PoolStub, this.MockSerializer(), "exchange", false, null, null);
        await publisher.DisposeAsync();

        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await publisher.PublishAsync("test"));
    }

    [Fact]
    public async Task CanDisposeMultipleTimes()
    {
        var mock = this.Mock();

        var publisher = new Next.Publisher.Publisher(mock.connection, this.PoolStub, this.MockSerializer(), "exchange", false,  null, null);
        await publisher.DisposeAsync();

        var ex = await Record.ExceptionAsync(async () => await publisher.DisposeAsync());

        Assert.Null(ex);
    }

    [Fact]
    public async Task DisposeShouldCloseChannel()
    {
        var mock = this.Mock();

        var publisher = new Next.Publisher.Publisher(mock.connection, this.PoolStub, this.MockSerializer(), "exchange", false, null, null);
        await publisher.InitializeAsync();
        await publisher.DisposeAsync();

        await mock.channel.Received().CloseAsync();
    }

    [Theory]
    [MemberData(nameof(PublishTestCases))]
    public async Task PublishAsync(
        IReadOnlyList<IMessageInitializer> transformers,
        string exchange, string routingKey, Action<IMessageBuilder> builder, PublishFlags flags,
        IMessageProperties expectedProperties)
    {
        var mock = this.Mock();

        var publisher = new Next.Publisher.Publisher(mock.connection, this.PoolStub, this.MockSerializer(), exchange, false, transformers, null);

        await publisher.PublishAsync("a", "test", (state, message) => builder?.Invoke(message), flags);

        await mock.channel.Received().PublishAsync(
            Arg.Any<(string, ISerializer, MessageBuilder)>(), exchange, routingKey,
            Arg.Is<IMessageProperties>(p => new MessagePropertiesComparer().Equals(expectedProperties, p)),
            Arg.Any<Action<(string, ISerializer, MessageBuilder), IBufferWriter<byte>>>(),
            flags, Arg.Any<CancellationToken>());
    }

    public static IEnumerable<object[]> PublishTestCases()
    {
        yield return new object[]
        {
            null,
            "myExchange", "key",
            (Action<IMessageBuilder>)(m => m.RoutingKey("key")),
            PublishFlags.None,
            new MessageProperties { DeliveryMode = DeliveryMode.Persistent }
        };

        yield return new object[]
        {
            null,
            "myExchange", "key",
            (Action<IMessageBuilder>)(m => m.RoutingKey("key")),
            PublishFlags.Immediate,
            new MessageProperties{ DeliveryMode = DeliveryMode.Persistent }
        };

        yield return new object[]
        {
            new IMessageInitializer[] { new UserIdInitializer("testUser")},
            "myExchange", "key",
            (Action<IMessageBuilder>)(m => m.RoutingKey("key")),
            PublishFlags.None,
            new MessageProperties { UserId = "testUser", DeliveryMode = DeliveryMode.Persistent}
        };

        yield return new object[]
        {
            null,
            "myExchange", "key",
            (Action<IMessageBuilder>)(m => m.RoutingKey("key").Priority(1).Type("test")),
            PublishFlags.None,
            new MessageProperties { Priority = 1, Type = "test", DeliveryMode = DeliveryMode.Persistent}
        };

        yield return new object[]
        {
            new IMessageInitializer[] { new UserIdInitializer("testUser")},
            "exchange", "key",
            (Action<IMessageBuilder>)(m =>m.RoutingKey("key").Priority(1).Type("test")),
            PublishFlags.None,
            new MessageProperties { Priority = 1, Type = "test", UserId = "testUser", DeliveryMode = DeliveryMode.Persistent}
        };
    }

    private ObjectPool<MessageBuilder> PoolStub = new DefaultObjectPool<MessageBuilder>(
        new NoResetMessagePropertiesPolicy());

    private (IConnection connection, IChannel channel) Mock()
    {
        var channel = Substitute.For<IChannel>();

        var connection = Substitute.For<IConnection>();
        connection.State.Returns(ConnectionState.Open);
        connection.OpenChannelAsync()
            .Returns(args => Task.FromResult(channel));

        return (connection, channel);
    }
    
    private ISerializer MockSerializer()
    {
        var serializer = Substitute.For<ISerializer>();
        serializer
            .When(x => x.Serialize( Arg.Any<IMessageProperties>(), Arg.Any<string>(), Arg.Any<IBufferWriter<byte>>()))
            .Do(x => x.ArgAt<IBufferWriter<byte>>(1).Write(new byte[] { 0x01, 0x02}));
        
        return serializer;
    }

    private class NoResetMessagePropertiesPolicy : IPooledObjectPolicy<MessageBuilder>
    {
        public MessageBuilder Create() => new();

        public bool Return(MessageBuilder obj) => false;
    }
}