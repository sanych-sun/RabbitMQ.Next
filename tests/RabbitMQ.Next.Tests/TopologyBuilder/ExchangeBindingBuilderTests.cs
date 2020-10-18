using System.Collections.Generic;
using RabbitMQ.Next.TopologyBuilder;
using RabbitMQ.Next.Transport.Methods.Exchange;
using Xunit;

namespace RabbitMQ.Next.Tests.TopologyBuilder
{
    public class ExchangeBindingBuilderTests
    {
        [Theory]
        [MemberData(nameof(TestCases))]
        public void ExchangeBindingBuilder(BindMethod expected, string exchange, string queue, string routingKey, IEnumerable<(string Key, object Value)> arguments)
        {
            var builder = new ExchangeBindingBuilder(exchange, queue)
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

        public static IEnumerable<object[]> TestCases()
        {
            var destination = "testQueue";
            var source = "exchange";

            yield return new object[] {new BindMethod(destination, source, string.Empty, null),
                source, destination, string.Empty, null};

            yield return new object[] {new BindMethod(destination, source, "route", null),
                source, destination, "route", null};

            yield return new object[] {new BindMethod(destination, source, "route", new Dictionary<string, object> { ["key"] = "value"}),
                source, destination, "route", new [] { ("key", (object)"value") } };

            yield return new object[] {new BindMethod(destination, source, "route", new Dictionary<string, object> { ["key"] = "value2"}),
                source, destination, "route", new [] { ("key", (object)"value1"), ("key", (object)"value2") } };

            yield return new object[] {new BindMethod(destination, source, "route", new Dictionary<string, object> { ["key1"] = "value1", ["key2"] = "value2"}),
                source, destination, "route", new [] { ("key1", (object)"value1"), ("key2", (object)"value2") } };
        }
    }
}