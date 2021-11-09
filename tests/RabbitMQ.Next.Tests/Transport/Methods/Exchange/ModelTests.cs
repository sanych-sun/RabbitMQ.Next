using System.Collections.Generic;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Transport.Methods.Exchange;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Exchange
{
    public class ModelTests
    {
        [Fact]
        public void DeclareMethod()
        {
            var name = "exchangeName";
            var type = "exchangeType";
            var flags = (byte)42;
            var arguments = new Dictionary<string, object>()
            {
                ["a"] = "a",
            };

            var method = new DeclareMethod(name, type, flags, arguments);

            Assert.Equal(MethodId.ExchangeDeclare, method.MethodId);
            Assert.Equal(name, method.Exchange);
            Assert.Equal(type, method.Type);
            Assert.Equal(flags, method.Flags);
            Assert.Equal(arguments, method.Arguments);
        }

        [Theory]
        [InlineData(0b_00000000, false, false, false)]
        [InlineData(0b_00000010, true, false, false)]
        [InlineData(0b_00000100, false, true, false)]
        [InlineData(0b_00001000, false, false, true)]
        public void DeclareMethodFlags(byte expected, bool durable, bool autoDelete, bool @internal)
        {
            var method = new DeclareMethod("exchange", "type", durable, autoDelete, @internal, null);

            Assert.Equal(expected, method.Flags);
        }

        [Fact]
        public void DeclarePassiveMethod()
        {
            var method = new DeclareMethod("exchange");

            Assert.Equal(0b_0000001, method.Flags);
            Assert.Null(method.Arguments);
            Assert.Equal("exchange", method.Exchange);
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