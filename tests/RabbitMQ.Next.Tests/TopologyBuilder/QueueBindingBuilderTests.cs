using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Exceptions;
using RabbitMQ.Next.Tests.Mocks;
using RabbitMQ.Next.TopologyBuilder;
using RabbitMQ.Next.Transport.Methods.Queue;
using Xunit;

namespace RabbitMQ.Next.Tests.TopologyBuilder
{
    public class QueueBindingBuilderTests
    {
        [Theory]
        [MemberData(nameof(TestCases))]
        public void QueueBindingBuilder(BindMethod expected, string queue, string exchange, string routingKey, IEnumerable<(string Key, object Value)> arguments)
        {
            var builder = new QueueBindingBuilder(queue, exchange)
            {
                RoutingKey = routingKey
            };

            if (arguments != null)
            {
                foreach (var arg in arguments)
                {
                    builder.SetArgument(arg.Key, arg.Value);
                }
            }

            var method = builder.ToMethod();

            Assert.Equal(expected.Exchange, method.Exchange);
            Assert.Equal(expected.Queue, method.Queue);
            Assert.Equal(expected.RoutingKey, method.RoutingKey);
            Assert.True(Helpers.DictionaryEquals(expected.Arguments, method.Arguments));
        }

        [Fact]
        public async Task ApplySendsMethod()
        {
            var channel = Substitute.For<IChannel>();
            var builder = new QueueBindingBuilder("queue", "exchange");
            var method = builder.ToMethod();

            await builder.ApplyAsync(channel);

            await channel.Received().SendAsync<BindMethod, BindOkMethod>(method);
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

        public static IEnumerable<object[]> TestCases()
        {
            var queue = "testQueue";
            var exchange = "exchange";

            yield return new object[] {new BindMethod(queue, exchange, string.Empty, null),
                queue, exchange, string.Empty, null};

            yield return new object[] {new BindMethod(queue, exchange, "route", null),
                queue, exchange, "route", null};

            yield return new object[] {new BindMethod(queue, exchange, "route", new Dictionary<string, object> { ["key"] = "value"}),
                queue, exchange, "route", new [] { ("key", (object)"value") } };

            yield return new object[] {new BindMethod(queue, exchange, "route", new Dictionary<string, object> { ["key"] = "value2"}),
                queue, exchange, "route", new [] { ("key", (object)"value1"), ("key", (object)"value2") } };

            yield return new object[] {new BindMethod(queue, exchange, "route", new Dictionary<string, object> { ["key1"] = "value1", ["key2"] = "value2"}),
                queue, exchange, "route", new [] { ("key1", (object)"value1"), ("key2", (object)"value2") } };
        }
    }
}