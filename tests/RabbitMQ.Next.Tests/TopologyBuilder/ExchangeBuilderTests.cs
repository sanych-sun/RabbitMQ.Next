using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Exceptions;
using RabbitMQ.Next.TopologyBuilder;
using RabbitMQ.Next.TopologyBuilder.Abstractions;
using RabbitMQ.Next.TopologyBuilder.Abstractions.Exceptions;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods.Exchange;
using Xunit;

namespace RabbitMQ.Next.Tests.TopologyBuilder
{
    public class ExchangeBuilderTests
    {
        [Theory]
        [MemberData(nameof(TestCases))]
        public void ExchangeBuilder(DeclareMethod expected, string exchange, string type, ExchangeFlags flags, IEnumerable<(string Key, object Value)> arguments)
        {
            var builder = new ExchangeBuilder(exchange, type)
            {
                Flags = flags
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
            Assert.Equal(expected.Type, method.Type);
            Assert.Equal(expected.Flags, method.Flags);
            Assert.True(Helpers.DictionaryEquals(expected.Arguments, method.Arguments));
        }

        [Fact]
        public async Task ApplySendsMethod()
        {
            var channel = Substitute.For<IChannel>();
            var builder = new ExchangeBuilder("exchange", ExchangeType.Direct);
            var method = builder.ToMethod();

            await builder.ApplyAsync(channel);

            await channel.Received().SendAsync<DeclareMethod, DeclareOkMethod>(method);
        }

        [Theory]
        [InlineData(ReplyCode.AccessRefused, typeof(ArgumentOutOfRangeException))]
        [InlineData(ReplyCode.PreconditionFailed, typeof(ArgumentOutOfRangeException))]
        [InlineData(ReplyCode.NotAllowed, typeof(ConflictException))]
        [InlineData(ReplyCode.CommandInvalid, typeof(NotSupportedException))]
        [InlineData(ReplyCode.ChannelError, typeof(ChannelException))]
        public async Task ApplyProcessExceptions(ReplyCode replyCode, Type exceptionType)
        {
            var channel = Substitute.For<IChannel>();
            channel.SendAsync<DeclareMethod, DeclareOkMethod>(default)
                .ReturnsForAnyArgs(Task.FromException<DeclareOkMethod>(new ChannelException((ushort)replyCode, "error message", (uint) MethodId.QueueBind)));
            var builder = new ExchangeBuilder("exchange", ExchangeType.Direct);

            await Assert.ThrowsAsync(exceptionType,async ()=> await builder.ApplyAsync(channel));
        }

        public static IEnumerable<object[]> TestCases()
        {
            var exchange = "exchange";

            yield return new object[] {new DeclareMethod(exchange, ExchangeType.Direct, (byte)ExchangeFlags.None, null),
                exchange, ExchangeType.Direct, ExchangeFlags.None, null};

            yield return new object[] {new DeclareMethod(exchange, ExchangeType.Direct, (byte)ExchangeFlags.Durable, null),
                exchange, ExchangeType.Direct, ExchangeFlags.Durable, null};

            yield return new object[] {new DeclareMethod(exchange, ExchangeType.Headers,(byte)(ExchangeFlags.Durable | ExchangeFlags.Internal), null),
                exchange, ExchangeType.Headers, ExchangeFlags.Durable | ExchangeFlags.Internal, null};

            yield return new object[] {new DeclareMethod(exchange, ExchangeType.Direct, (byte)ExchangeFlags.None, new Dictionary<string, object> { ["key"] = "value"}),
                exchange, ExchangeType.Direct, ExchangeFlags.None, new [] { ("key", (object)"value") } };

            yield return new object[] {new DeclareMethod(exchange, ExchangeType.Direct, (byte)ExchangeFlags.None, new Dictionary<string, object> { ["key"] = "value2"}),
                exchange, ExchangeType.Direct, ExchangeFlags.None, new [] { ("key", (object)"value1"), ("key", (object)"value2") } };

            yield return new object[] {new DeclareMethod(exchange, ExchangeType.Direct, (byte)ExchangeFlags.None, new Dictionary<string, object> { ["key1"] = "value1", ["key2"] = "value2"}),
                exchange, ExchangeType.Direct, ExchangeFlags.None, new [] { ("key1", (object)"value1"), ("key2", (object)"value2") } };
        }
    }
}