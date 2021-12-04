using System;
using NSubstitute;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Serialization.Abstractions;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher
{
    public class PublisherBuilderTests
    {
        [Fact]
        public void CanRegisterTransformers()
        {
            var transformer1 = Substitute.For<IMessageInitializer>();
            var transformer2 = Substitute.For<IMessageInitializer>();

            var builder = new PublisherBuilder();
            ((IPublisherBuilder) builder).UseMessageInitializer(transformer1);
            ((IPublisherBuilder) builder).UseMessageInitializer(transformer2);

            Assert.Contains(transformer1, builder.Initializers);
            Assert.Contains(transformer2, builder.Initializers);
        }

        [Fact]
        public void ThrowsOnInvalidTransformer()
        {
            var builder = new PublisherBuilder();
            
            Assert.Throws<ArgumentNullException>(() => ((IPublisherBuilder)builder).UseMessageInitializer(null));
        }
        
        [Fact]
        public void CanRegisterReturnedMessageHandlers()
        {
            var transformer1 = Substitute.For<IReturnedMessageHandler>();
            var transformer2 = Substitute.For<IReturnedMessageHandler>();

            var builder = new PublisherBuilder();
            ((IPublisherBuilder) builder).AddReturnedMessageHandler(transformer1);
            ((IPublisherBuilder) builder).AddReturnedMessageHandler(transformer2);

            Assert.Contains(transformer1, builder.ReturnedMessageHandlers);
            Assert.Contains(transformer2, builder.ReturnedMessageHandlers);
        }

        [Fact]
        public void ThrowsOnInvalidReturnedMessageHandler()
        {
            var builder = new PublisherBuilder();
            
            Assert.Throws<ArgumentNullException>(() => ((IPublisherBuilder)builder).AddReturnedMessageHandler(null));
        }

        [Fact]
        public void ConfirmsDefault()
        {
            var builder = new PublisherBuilder();

            Assert.False(builder.PublisherConfirms);
        }

        [Fact]
        public void Confirms()
        {
            var builder = new PublisherBuilder();
            ((IPublisherBuilder)builder).PublisherConfirms();

            Assert.True(builder.PublisherConfirms);
        }

        [Fact]
        public void UseSerializer()
        {
            var serializer = Substitute.For<ISerializer>();
            var contentType = "application/json";
            var isDefault = false;

            var builder = new PublisherBuilder();

            builder.UseSerializer(serializer, contentType, isDefault);

            Assert.Contains(builder.Serializers, s => s.Serializer == serializer && s.ContentType == contentType && s.Default == isDefault);
        }

        [Fact]
        public void UseSerializerThrowsOnNull()
        {
            var builder = new PublisherBuilder();
            Assert.Throws<ArgumentNullException>(() => builder.UseSerializer(null));
        }
    }
}