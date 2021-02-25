using System.Collections.Generic;
using NSubstitute;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;
using RabbitMQ.Next.Publisher.Attributes;
using Xunit;

[assembly:ApplicationId("unitTest")]

namespace RabbitMQ.Next.Tests.Publisher.Attributes
{
    public class AttributeTransformerTests
    {
        [Fact]
        public void TestAssemblyAttributes()
        {
            var attributeTransformer = new AttributeTransformer();
            var message = Substitute.For<IMessageBuilder>();

            attributeTransformer.Apply(new AssemblyAttributesData(), message);

            message.Received().SetApplicationId("unitTest");
        }

        [Fact]
        public void TestExchangeAttribute()
        {
            var attributeTransformer = new AttributeTransformer();
            var message = Substitute.For<IMessageBuilder>();

            attributeTransformer.Apply(new ExchangeAttributeData(), message);

            message.Received().SetExchange("test");
        }

        [Fact]
        public void TestRoutingKeyAttribute()
        {
            var attributeTransformer = new AttributeTransformer();
            var message = Substitute.For<IMessageBuilder>();

            attributeTransformer.Apply(new RoutingKeyAttributeData(), message);

            message.Received().SetRoutingKey("route");
        }

        [Fact]
        public void TestHeaderAttribute()
        {
            var attributeTransformer = new AttributeTransformer();
            var message = Substitute.For<IMessageBuilder>();
            message.Headers.Returns(new Dictionary<string, object>());

            attributeTransformer.Apply(new HeaderAttributeData(), message);

            message.Received().SetHeader("headerA", "value1");
            message.Received().SetHeader("headerB", "value2");
        }

        [Fact]
        public void TestMultipleAttributes()
        {
            var attributeTransformer = new AttributeTransformer();
            var message = Substitute.For<IMessageBuilder>();

            attributeTransformer.Apply(new MultipleAttributesData(), message);

            message.Received().SetExchange("test");
            message.Received().SetRoutingKey("route");
            message.Received().SetPriority(7);
        }

        private class AssemblyAttributesData
        {

        }

        [Exchange("test")]
        private class ExchangeAttributeData
        {
        }

        [RoutingKey("route")]
        private class RoutingKeyAttributeData
        {
        }

        [Header("headerA", "value1")]
        [Header("headerB", "value2")]
        private class HeaderAttributeData
        {
        }

        [Exchange("test")]
        [RoutingKey("route")]
        [Priority(7)]
        private class MultipleAttributesData
        {
        }
    }
}