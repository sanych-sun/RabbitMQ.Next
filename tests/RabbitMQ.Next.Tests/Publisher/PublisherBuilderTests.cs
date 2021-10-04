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
        public void AddSerializer()
        {
            var consumerBuilder = new PublisherBuilder();
            var serializer1 = Substitute.For<ISerializer>();
            var serializer2 = Substitute.For<ISerializer>();

            ((IPublisherBuilder) consumerBuilder).AddSerializer(serializer1, "test1");
            ((IPublisherBuilder) consumerBuilder).AddSerializer(serializer2, "test2");

            Assert.Equal(serializer1, consumerBuilder.Serializers["test1"]);
            Assert.Equal(serializer2, consumerBuilder.Serializers["test2"]);
        }
        
        [Fact]
        public void AddSerializerAsDefault()
        {
            var consumerBuilder = new PublisherBuilder();
            var serializer1 = Substitute.For<ISerializer>();

            ((IPublisherBuilder) consumerBuilder).AddSerializer(serializer1);

            Assert.Equal(serializer1, consumerBuilder.Serializers[string.Empty]);
        }

        [Fact]
        public void AddSerializerCanOverride()
        {
            var consumerBuilder = new PublisherBuilder();
            var serializer1 = Substitute.For<ISerializer>();
            var serializer2 = Substitute.For<ISerializer>();

            ((IPublisherBuilder) consumerBuilder).AddSerializer(serializer1, "test1");
            ((IPublisherBuilder) consumerBuilder).AddSerializer(serializer2, "test1");

            Assert.Equal(serializer2, consumerBuilder.Serializers["test1"]);
        }

        [Fact]
        public void AddSerializerCanOverrideDefault()
        {
            var consumerBuilder = new PublisherBuilder();
            var serializer1 = Substitute.For<ISerializer>();
            var serializer2 = Substitute.For<ISerializer>();

            ((IPublisherBuilder) consumerBuilder).AddSerializer(serializer1);
            ((IPublisherBuilder) consumerBuilder).AddSerializer(serializer2);

            Assert.Equal(serializer2, consumerBuilder.Serializers[string.Empty]);
        }

        [Fact]
        public void AddSerializerThrowsOnNull()
        {
            var consumerBuilder = new PublisherBuilder();

            Assert.Throws<ArgumentNullException>(() => ((IPublisherBuilder) consumerBuilder).AddSerializer(null));
        }

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
    }
}