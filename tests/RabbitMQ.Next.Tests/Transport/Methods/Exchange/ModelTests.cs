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

            Assert.Equal((uint)MethodId.ExchangeDeclare, method.Method);
            Assert.Equal(name, method.Exchange);
            Assert.Equal(type, method.Type);
            Assert.Equal(flags, method.Flags);
            Assert.Equal(arguments, method.Arguments);
        }

        [Theory]
        [InlineData(0b_00000000, false, false, false, false, false)]
        [InlineData(0b_00000001, true, false, false, false, false)]
        [InlineData(0b_00000010, false, true, false, false, false)]
        [InlineData(0b_00000100, false, false, true, false, false)]
        [InlineData(0b_00001000, false, false, false, true, false)]
        [InlineData(0b_00010000, false, false, false, false, true)]
        [InlineData(0b_00011111, true, true, true, true, true)]
        public void DeclareMethodFlags(byte expected, bool passive, bool durable, bool autoDelete, bool @internal, bool nowait)
        {
            var method = new DeclareMethod("exchange", "type", passive, durable, autoDelete, @internal, nowait, null);

            Assert.Equal(expected, method.Flags);
        }

        [Fact]
        public void DeclareOkMethod()
        {
            var method = new DeclareOkMethod();

            Assert.Equal((uint)MethodId.ExchangeDeclareOk, method.Method);
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

            Assert.Equal((uint)MethodId.ExchangeBind, method.Method);
            Assert.Equal(destination, method.Destination);
            Assert.Equal(source, method.Source);
            Assert.Equal(routingKey, method.RoutingKey);
            Assert.Equal(arguments, method.Arguments);
        }

        [Fact]
        public void BindOkMethod()
        {
            var method = new BindOkMethod();

            Assert.Equal((uint)MethodId.ExchangeBindOk, method.Method);
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

            Assert.Equal((uint)MethodId.ExchangeUnbind, method.Method);
            Assert.Equal(destination, method.Destination);
            Assert.Equal(source, method.Source);
            Assert.Equal(routingKey, method.RoutingKey);
            Assert.Equal(arguments, method.Arguments);
        }

        [Fact]
        public void UnbindOkMethod()
        {
            var method = new UnbindOkMethod();

            Assert.Equal((uint)MethodId.ExchangeUnbindOk, method.Method);
        }

        [Fact]
        public void DeleteMethod()
        {
            var name = "exchangeName";
            var unusedOnly = true;

            var method = new DeleteMethod(name, unusedOnly);

            Assert.Equal((uint)MethodId.ExchangeDelete, method.Method);
            Assert.Equal(name, method.Exchange);
            Assert.Equal(unusedOnly, method.UnusedOnly);
        }

        [Fact]
        public void DeleteOkMethod()
        {
            var method = new DeleteOkMethod();

            Assert.Equal((uint)MethodId.ExchangeDeleteOk, method.Method);
        }
    }
}