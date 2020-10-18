using System.Collections.Generic;
using System.Linq;
using RabbitMQ.Next.TopologyBuilder;
using RabbitMQ.Next.TopologyBuilder.Abstractions;
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
        public void BindWithoutCustomization()
        {
            var source = "source";
            var exchange = "exchange";

            var builder = new ExchangeBuilder(exchange, string.Empty);

            builder.BindTo(source);

            var item = builder.Bindings.Single();
            Assert.Equal(exchange, item.Destination);
            Assert.Equal(source, item.Source);
            Assert.Equal(null, item.RoutingKey);
        }

        [Fact]
        public void BindWithCustomization()
        {
            var source = "source";
            var exchange = "exchange";
            var routingKey = "test";

            var builder = new ExchangeBuilder(exchange, string.Empty);

            builder.BindTo(source, binding => binding.RoutingKey = routingKey);

            var item = builder.Bindings.Single();
            Assert.Equal(exchange, item.Destination);
            Assert.Equal(source, item.Source);
            Assert.Equal(routingKey, item.RoutingKey);
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