using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Exceptions;
using RabbitMQ.Next.TopologyBuilder;
using RabbitMQ.Next.Transport.Methods.Exchange;
using Xunit;

namespace RabbitMQ.Next.Tests.TopologyBuilder
{
    public class ExchangeBindingBuilderTests
    {
        [Theory]
        [MemberData(nameof(TestCases))]
        public void ExchangeBindingBuilder(BindMethod expected, string destination, string source, string routingKey, IEnumerable<(string Key, object Value)> arguments)
        {
            var builder = new ExchangeBindingBuilder(destination, source)
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

            Assert.Equal(expected.Source, method.Source);
            Assert.Equal(expected.Destination, method.Destination);
            Assert.Equal(expected.RoutingKey, method.RoutingKey);
            Assert.True(Helpers.DictionaryEquals(expected.Arguments, method.Arguments));
        }

        [Fact]
        public async Task ApplySendsMethod()
        {
            var channel = Substitute.For<IChannel>();
            var builder = new ExchangeBindingBuilder("destination", "source");
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
                .ReturnsForAnyArgs(Task.FromException<BindOkMethod>(new ChannelException((ushort)replyCode, "error message", MethodId.ExchangeBind)));
            var builder = new ExchangeBindingBuilder("destination", "source");

            await Assert.ThrowsAsync(exceptionType,async ()=> await builder.ApplyAsync(channel));
        }

        public static IEnumerable<object[]> TestCases()
        {
            var destination = "testQueue";
            var source = "exchange";

            yield return new object[] {new BindMethod(destination, source, string.Empty, null),
                destination, source, string.Empty, null};

            yield return new object[] {new BindMethod(destination, source, "route", null),
                destination, source, "route", null};

            yield return new object[] {new BindMethod(destination, source, "route", new Dictionary<string, object> { ["key"] = "value"}),
                destination, source, "route", new [] { ("key", (object)"value") } };

            yield return new object[] {new BindMethod(destination, source, "route", new Dictionary<string, object> { ["key"] = "value2"}),
                destination, source, "route", new [] { ("key", (object)"value1"), ("key", (object)"value2") } };

            yield return new object[] {new BindMethod(destination, source, "route", new Dictionary<string, object> { ["key1"] = "value1", ["key2"] = "value2"}),
                destination, source, "route", new [] { ("key1", (object)"value1"), ("key2", (object)"value2") } };
        }
    }
}