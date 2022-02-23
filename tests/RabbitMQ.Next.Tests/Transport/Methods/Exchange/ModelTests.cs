using System.Collections.Generic;
using RabbitMQ.Next.Transport.Methods.Exchange;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Exchange
{
    public class ModelTests
    {
        [Theory]
        [MemberData(nameof(DeclareTestCases))]
        public void DeclareMethod(string name, string type, bool durable, bool autoDelete, bool @internal, IReadOnlyDictionary<string, object> arguments)
        {
            var method = new DeclareMethod(name, type, durable, autoDelete, @internal, arguments);

            Assert.Equal(MethodId.ExchangeDeclare, method.MethodId);
            Assert.Equal(name, method.Exchange);
            Assert.Equal(type, method.Type);
            Assert.False(method.Passive);
            Assert.Equal(durable, method.Durable);
            Assert.Equal(autoDelete, method.AutoDelete);
            Assert.Equal(@internal, method.Internal);
            Assert.Equal(arguments, method.Arguments);
        }

        public static IEnumerable<object[]> DeclareTestCases()
        {
            yield return new object[] {"exchangeName", "type", false, false, false, null};
            yield return new object[] { "exchangeName", "type", true, false, false, null};
            yield return new object[] { "exchangeName", "type", false, true, false, null};
            yield return new object[] { "exchangeName", "type", false, false, true, null};
            yield return new object[] { "exchangeName", "type", true, true, true, null};
            yield return new object[] { "exchangeName", "type", true, true, true, new Dictionary<string, object>()
            {
                ["a"] = "a",
            }};
        }

        [Fact]
        public void DeclarePassiveMethod()
        {
            var name = "exchangeName";

            var method = new DeclareMethod(name);

            Assert.Equal(MethodId.ExchangeDeclare, method.MethodId);
            Assert.Equal(name, method.Exchange);
            Assert.True(method.Passive);
            Assert.False(method.Durable);
            Assert.False(method.AutoDelete);
            Assert.False(method.Internal);
            Assert.Null(method.Type);
            Assert.Null(method.Arguments);
        }

        [Fact]
        public void DeclareOkMethod()
        {
            var method = new DeclareOkMethod();

            Assert.Equal(MethodId.ExchangeDeclareOk, method.MethodId);
        }

        [Fact]
        public void BindMethod()
        {
            var destination = "destination";
            var source = "source";
            var routingKey = "routingKey";
            var arguments = new Dictionary<string, object>()
            {
                ["a"] = "a",
            };

            var method = new BindMethod(destination, source, routingKey, arguments);

            Assert.Equal(MethodId.ExchangeBind, method.MethodId);
            Assert.Equal(destination, method.Destination);
            Assert.Equal(source, method.Source);
            Assert.Equal(routingKey, method.RoutingKey);
            Assert.Equal(arguments, method.Arguments);
        }

        [Fact]
        public void BindOkMethod()
        {
            var method = new BindOkMethod();

            Assert.Equal(MethodId.ExchangeBindOk, method.MethodId);
        }

        [Fact]
        public void UnbindMethod()
        {
            var destination = "destination";
            var source = "source";
            var routingKey = "routingKey";
            var arguments = new Dictionary<string, object>()
            {
                ["a"] = "a",
            };

            var method = new UnbindMethod(destination, source, routingKey, arguments);

            Assert.Equal(MethodId.ExchangeUnbind, method.MethodId);
            Assert.Equal(destination, method.Destination);
            Assert.Equal(source, method.Source);
            Assert.Equal(routingKey, method.RoutingKey);
            Assert.Equal(arguments, method.Arguments);
        }

        [Fact]
        public void UnbindOkMethod()
        {
            var method = new UnbindOkMethod();

            Assert.Equal(MethodId.ExchangeUnbindOk, method.MethodId);
        }

        [Fact]
        public void DeleteMethod()
        {
            var name = "exchangeName";
            var unusedOnly = true;

            var method = new DeleteMethod(name, unusedOnly);

            Assert.Equal(MethodId.ExchangeDelete, method.MethodId);
            Assert.Equal(name, method.Exchange);
            Assert.Equal(unusedOnly, method.UnusedOnly);
        }

        [Fact]
        public void DeleteOkMethod()
        {
            var method = new DeleteOkMethod();

            Assert.Equal(MethodId.ExchangeDeleteOk, method.MethodId);
        }
    }
}