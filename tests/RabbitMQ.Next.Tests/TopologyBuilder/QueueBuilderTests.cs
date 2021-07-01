using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Exceptions;
using RabbitMQ.Next.Tests.Mocks;
using RabbitMQ.Next.TopologyBuilder;
using RabbitMQ.Next.TopologyBuilder.Abstractions;
using RabbitMQ.Next.TopologyBuilder.Abstractions.Exceptions;
using RabbitMQ.Next.Transport.Methods.Queue;
using Xunit;

namespace RabbitMQ.Next.Tests.TopologyBuilder
{
    public class QueueBuilderTests
    {
        [Theory]
        [MemberData(nameof(TestCases))]
        public void QueueBuilder(DeclareMethod expected, string queue, QueueFlags flags, IEnumerable<(string Key, object Value)> arguments)
        {
            var builder = new QueueBuilder(queue)
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

            Assert.Equal(expected.Queue, method.Queue);
            Assert.Equal(expected.Flags, method.Flags);
            Assert.True(Helpers.DictionaryEquals(expected.Arguments, method.Arguments));
        }

        [Fact]
        public async Task ApplySendsMethod()
        {
            var channel = Substitute.For<IChannel>();
            var builder = new QueueBuilder("queue");
            var method = builder.ToMethod();

            await builder.ApplyAsync(channel);

            await channel.Received().SendAsync<DeclareMethod, DeclareOkMethod>(method);
        }

        [Theory]
        [InlineData(ReplyCode.AccessRefused, typeof(ArgumentOutOfRangeException))]
        [InlineData(ReplyCode.PreconditionFailed, typeof(ArgumentOutOfRangeException))]
        [InlineData(ReplyCode.ResourceLocked, typeof(ConflictException))]
        [InlineData(ReplyCode.ChannelError, typeof(ChannelException))]
        public async Task ApplyProcessExceptions(ReplyCode replyCode, Type exceptionType)
        {
            var channel = Substitute.For<IChannel>();
            channel.SendAsync<DeclareMethod, DeclareOkMethod>(default)
                .ReturnsForAnyArgs(new ValueTask<DeclareOkMethod>(Task.FromException<DeclareOkMethod>(new ChannelException((ushort)replyCode, "error message", MethodId.QueueBind))));
            var builder = new QueueBuilder("queue");

            await Assert.ThrowsAsync(exceptionType,async ()=> await builder.ApplyAsync(channel));
        }

        public static IEnumerable<object[]> TestCases()
        {
            var queue = "testQueue";

            yield return new object[] {new DeclareMethod(queue, (byte)QueueFlags.None, null),
                queue, QueueFlags.None, null};

            yield return new object[] {new DeclareMethod(queue, (byte)QueueFlags.Durable, null),
                queue, QueueFlags.Durable, null};

            yield return new object[] {new DeclareMethod(queue, (byte)(QueueFlags.AutoDelete | QueueFlags.Exclusive), null),
                queue, QueueFlags.AutoDelete | QueueFlags.Exclusive, null};

            yield return new object[] {new DeclareMethod(queue, (byte)QueueFlags.None, new Dictionary<string, object> { ["key"] = "value"}),
                queue, QueueFlags.None, new [] { ("key", (object)"value") } };

            yield return new object[] {new DeclareMethod(queue, (byte)QueueFlags.None, new Dictionary<string, object> { ["key"] = "value2"}),
                queue, QueueFlags.None, new [] { ("key", (object)"value1"), ("key", (object)"value2") } };

            yield return new object[] {new DeclareMethod(queue, (byte)QueueFlags.None, new Dictionary<string, object> { ["key1"] = "value1", ["key2"] = "value2"}),
                queue, QueueFlags.None, new [] { ("key1", (object)"value1"), ("key2", (object)"value2") } };
        }
    }
}