using System.Collections.Generic;
using RabbitMQ.Next.TopologyBuilder;
using RabbitMQ.Next.Transport.Methods.Queue;
using Xunit;

namespace RabbitMQ.Next.Tests.TopologyBuilder
{
    public class QueueBindingBuilderTests
    {
        [Theory]
        [MemberData(nameof(TestCases))]
        public void QueueBindingBuilder(BindMethod expected, string exchange, string queue, string routingKey, IEnumerable<(string Key, object Value)> arguments)
        {
            var builder = new QueueBindingBuilder(exchange, queue)
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

        public static IEnumerable<object[]> TestCases()
        {
            var queue = "testQueue";
            var exchange = "exchange";

            yield return new object[] {new BindMethod(queue, exchange, string.Empty, null),
                exchange, queue, string.Empty, null};

            yield return new object[] {new BindMethod(queue, exchange, "route", null),
                exchange, queue, "route", null};

            yield return new object[] {new BindMethod(queue, exchange, "route", new Dictionary<string, object> { ["key"] = "value"}),
                exchange, queue, "route", new [] { ("key", (object)"value") } };

            yield return new object[] {new BindMethod(queue, exchange, "route", new Dictionary<string, object> { ["key"] = "value2"}),
                exchange, queue, "route", new [] { ("key", (object)"value1"), ("key", (object)"value2") } };

            yield return new object[] {new BindMethod(queue, exchange, "route", new Dictionary<string, object> { ["key1"] = "value1", ["key2"] = "value2"}),
                exchange, queue, "route", new [] { ("key1", (object)"value1"), ("key2", (object)"value2") } };
        }
    }
}