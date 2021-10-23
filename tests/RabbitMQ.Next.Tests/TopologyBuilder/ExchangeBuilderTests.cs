using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Exceptions;
using RabbitMQ.Next.TopologyBuilder;
using RabbitMQ.Next.TopologyBuilder.Exceptions;
using RabbitMQ.Next.TopologyBuilder.Builders;
using RabbitMQ.Next.Transport.Methods.Exchange;
using Xunit;

namespace RabbitMQ.Next.Tests.TopologyBuilder
{
    public class ExchangeBuilderTests
    {
        [Fact]
        public async Task ApplySendsMethod()
        {
            var channel = Substitute.For<IChannel>();
            var builder = new ExchangeBuilder("exchange", ExchangeType.Direct);

            await builder.ApplyAsync(channel);

            await channel.Received().SendAsync<DeclareMethod, DeclareOkMethod>(Arg.Any<DeclareMethod>());
        }

        [Fact]
        public async Task CanPassExchangeName()
        {
            var exchangeName = "test-exch";
            var channel = Substitute.For<IChannel>();
            var builder = new ExchangeBuilder(exchangeName, ExchangeType.Direct);

            await builder.ApplyAsync(channel);

            await channel.Received().SendAsync<DeclareMethod, DeclareOkMethod>(Arg.Is<DeclareMethod>(
                m => m.Exchange == exchangeName));
        }

        [Fact]
        public async Task CanPassExchangeType()
        {
            var channel = Substitute.For<IChannel>();
            var builder = new ExchangeBuilder("exchange", ExchangeType.Topic);

            await builder.ApplyAsync(channel);

            await channel.Received().SendAsync<DeclareMethod, DeclareOkMethod>(Arg.Is<DeclareMethod>(
                m => m.Type == ExchangeType.Topic));
        }

        [Fact]
        public async Task CanPassFlag()
        {
            var channel = Substitute.For<IChannel>();
            var builder = new ExchangeBuilder("exchange", ExchangeType.Direct);
            builder.Flags(ExchangeFlags.Durable);

            await builder.ApplyAsync(channel);

            await channel.Received().SendAsync<DeclareMethod, DeclareOkMethod>(Arg.Is<DeclareMethod>(
                m => m.Flags == (byte)ExchangeFlags.Durable));
        }

        [Fact]
        public async Task CanCombineFlags()
        {
            var channel = Substitute.For<IChannel>();
            var builder = new ExchangeBuilder("exchange", ExchangeType.Direct);
            builder.Flags(ExchangeFlags.AutoDelete);
            builder.Flags(ExchangeFlags.Internal);

            await builder.ApplyAsync(channel);

            await channel.Received().SendAsync<DeclareMethod, DeclareOkMethod>(Arg.Is<DeclareMethod>(
                m => m.Flags == (byte)(ExchangeFlags.AutoDelete | ExchangeFlags.Internal)));
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
            var builder = new ExchangeBuilder("exchange", ExchangeType.Direct);

            foreach (var arg in arguments)
            {
                builder.Argument(arg.Key, arg.Value);
            }

            await builder.ApplyAsync(channel);

            await channel.Received().SendAsync<DeclareMethod, DeclareOkMethod>(Arg.Is<DeclareMethod>(
                m => arguments.All(a => m.Arguments[a.Key] == a.Value)));
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
                .ReturnsForAnyArgs(new ValueTask<DeclareOkMethod>(Task.FromException<DeclareOkMethod>(new ChannelException((ushort)replyCode, "error message", MethodId.QueueBind))));
            var builder = new ExchangeBuilder("exchange", ExchangeType.Direct);

            await Assert.ThrowsAsync(exceptionType,async ()=> await builder.ApplyAsync(channel));
        }
    }
}