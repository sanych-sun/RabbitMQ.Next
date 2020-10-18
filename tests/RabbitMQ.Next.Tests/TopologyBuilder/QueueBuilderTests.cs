using System.Collections.Generic;
using System.Linq;
using RabbitMQ.Next.TopologyBuilder;
using RabbitMQ.Next.TopologyBuilder.Abstractions;
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
        public void BindWithoutCustomization()
        {
            var queue = "queue";
            var exchange = "exchange";

            var builder = new QueueBuilder(queue);

            builder.BindTo(exchange);

            var item = builder.Bindings.Single();
            Assert.Equal(queue, item.Queue);
            Assert.Equal(exchange, item.Exchange);
            Assert.Equal(null, item.RoutingKey);
        }

        [Fact]
        public void BindWithCustomization()
        {
            var queue = "queue";
            var exchange = "exchange";
            var routingKey = "test";

            var builder = new QueueBuilder(queue);

            builder.BindTo(exchange, binding => binding.RoutingKey = routingKey);

            var item = builder.Bindings.Single();
            Assert.Equal(queue, item.Queue);
            Assert.Equal(exchange, item.Exchange);
            Assert.Equal(routingKey, item.RoutingKey);
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