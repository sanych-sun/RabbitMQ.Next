using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Exceptions;
using RabbitMQ.Next.Tests.Mocks;
using RabbitMQ.Next.TopologyBuilder;
using RabbitMQ.Next.TopologyBuilder.Builders;
using RabbitMQ.Next.Transport.Methods.Queue;
using Xunit;

namespace RabbitMQ.Next.Tests.TopologyBuilder
{
    public class QueueBindingBuilderTests
    {
        [Fact]
        public async Task ApplySendsMethod()
        {
            var channel = Substitute.For<IChannel>();
            var builder = new QueueBindingBuilder("queue", "exchange");

            await builder.ApplyAsync(channel);

            await channel.Received().SendAsync<BindMethod, BindOkMethod>(Arg.Any<BindMethod>());
        }

        [Fact]
        public async Task CanPassNames()
        {
            var queueName = "test-queue";
            var exchangeName = "test-exchange";
            var channel = Substitute.For<IChannel>();
            var builder = new QueueBindingBuilder(queueName, exchangeName);

            await builder.ApplyAsync(channel);

            await channel.Received().SendAsync<BindMethod, BindOkMethod>(Arg.Is<BindMethod>(
                m => m.Queue == queueName && m.Exchange == exchangeName));
        }

        [Fact]
        public async Task CanPassRoutingKey()
        {
            var routingKey = "route";
            var channel = Substitute.For<IChannel>();
            var builder = new QueueBindingBuilder("queue", "exchange");
            builder.RoutingKey(routingKey);

            await builder.ApplyAsync(channel);

            await channel.Received().SendAsync<BindMethod, BindOkMethod>(Arg.Is<BindMethod>(
                m => m.RoutingKey == routingKey));
        }

        [Fact]
        public async Task CanPassMultipleRoutingKeys()
        {
            var keys = new List<string>
            {
                "key",
                "other-key"
            };
            var channel = Substitute.For<IChannel>();
            var builder = new QueueBindingBuilder("queue", "exchange");
            foreach (var key in keys)
            {
                builder.RoutingKey(key);
            }

            await builder.ApplyAsync(channel);

            foreach (var key in keys)
            {
                await channel.Received().SendAsync<BindMethod, BindOkMethod>(Arg.Is<BindMethod>(
                    m => m.RoutingKey == key));
            }
        }

        [Fact]
        public async Task CanPassArguments()
        {
            var arguments = new Dictionary<string, object>()
            {
                ["test"] = "value",
                ["key2"] = 5,
            };
            var channel = Substitute.For<IChannel>();
            var builder = new QueueBindingBuilder("queue", "exchange");
            foreach (var argument in arguments)
            {
                builder.Argument(argument.Key, argument.Value);
            }

            await builder.ApplyAsync(channel);

            await channel.Received().SendAsync<BindMethod, BindOkMethod>(Arg.Is<BindMethod>(
                m => arguments.All(a => m.Arguments[a.Key] == a.Value)));
        }

        [Theory]
        [InlineData(ReplyCode.NotFound, typeof(ArgumentOutOfRangeException))]
        [InlineData(ReplyCode.ChannelError, typeof(ChannelException))]
        public async Task ApplyProcessExceptions(ReplyCode replyCode, Type exceptionType)
        {
            var channel = Substitute.For<IChannel>();
            channel.SendAsync<BindMethod, BindOkMethod>(default)
                .ReturnsForAnyArgs(new ValueTask<BindOkMethod>(Task.FromException<BindOkMethod>(new ChannelException((ushort)replyCode, "error message", MethodId.QueueBind))));
            var builder = new QueueBindingBuilder("queue", "exchange");

            await Assert.ThrowsAsync(exceptionType,async ()=> await builder.ApplyAsync(channel));
        }
    }
}