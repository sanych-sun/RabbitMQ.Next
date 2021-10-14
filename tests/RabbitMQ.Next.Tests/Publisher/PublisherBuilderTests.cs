using System;
using NSubstitute;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Serialization.Abstractions;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher
{
    public class PublisherBuilderTests
    {
        [Fact]
        public void ThrowsOnEmptyFactory()
        {
            Assert.Throws<ArgumentNullException>(() => new PublisherBuilder(null));
        }

        [Fact]
        public void ReturnsFactory()
        {
            var serializerFactory = Substitute.For<ISerializerFactory>();

            var builder = new PublisherBuilder(serializerFactory);
            Assert.Equal(serializerFactory, builder.SerializerFactory);
        }

        [Fact]
        public void CanRegisterTransformers()
        {
            var transformer1 = Substitute.For<IMessageInitializer>();
            var transformer2 = Substitute.For<IMessageInitializer>();
            var serializerFactory = Substitute.For<ISerializerFactory>();

            var builder = new PublisherBuilder(serializerFactory);
            ((IPublisherBuilder) builder).UseMessageInitializer(transformer1);
            ((IPublisherBuilder) builder).UseMessageInitializer(transformer2);

            Assert.Contains(transformer1, builder.Initializers);
            Assert.Contains(transformer2, builder.Initializers);
        }

        [Fact]
        public void ThrowsOnInvalidTransformer()
        {
            var serializerFactory = Substitute.For<ISerializerFactory>();
            var builder = new PublisherBuilder(serializerFactory);
            
            Assert.Throws<ArgumentNullException>(() => ((IPublisherBuilder)builder).UseMessageInitializer(null));
        }
        
        [Fact]
        public void CanRegisterReturnedMessageHandlers()
        {
            var serializerFactory = Substitute.For<ISerializerFactory>();
            var transformer1 = Substitute.For<IReturnedMessageHandler>();
            var transformer2 = Substitute.For<IReturnedMessageHandler>();

            var builder = new PublisherBuilder(serializerFactory);
            ((IPublisherBuilder) builder).AddReturnedMessageHandler(transformer1);
            ((IPublisherBuilder) builder).AddReturnedMessageHandler(transformer2);

            Assert.Contains(transformer1, builder.ReturnedMessageHandlers);
            Assert.Contains(transformer2, builder.ReturnedMessageHandlers);
        }

        [Fact]
        public void ThrowsOnInvalidReturnedMessageHandler()
        {
            var serializerFactory = Substitute.For<ISerializerFactory>();
            var builder = new PublisherBuilder(serializerFactory);
            
            Assert.Throws<ArgumentNullException>(() => ((IPublisherBuilder)builder).AddReturnedMessageHandler(null));
        }

        [Fact]
        public void ConfirmsDefault()
        {
            var serializerFactory = Substitute.For<ISerializerFactory>();
            var builder = new PublisherBuilder(serializerFactory);

            Assert.False(builder.PublisherConfirms);
        }

        [Fact]
        public void Confirms()
        {
            var serializerFactory = Substitute.For<ISerializerFactory>();
            var builder = new PublisherBuilder(serializerFactory);
            ((IPublisherBuilder)builder).PublisherConfirms();

            Assert.True(builder.PublisherConfirms);
        }

        [Fact]
        public void RegisterSerializerCallFactory()
        {
            var serializerFactory = Substitute.For<ISerializerFactory>();
            var builder = new PublisherBuilder(serializerFactory);
            var serializer = Substitute.For<ISerializer>();

            var types = new[] { "type" };
            builder.UseSerializer(serializer, types);

            serializerFactory.Received().RegisterSerializer(serializer, types);
        }
    }
}