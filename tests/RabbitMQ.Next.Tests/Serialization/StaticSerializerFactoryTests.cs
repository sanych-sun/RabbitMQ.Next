using System;
using NSubstitute;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Serialization.Abstractions;
using Xunit;

namespace RabbitMQ.Next.Tests.Serialization
{
    public class StaticSerializerFactoryTests
    {
        [Fact]
        public void ThrowsOnNullSerializer()
        {
            Assert.Throws<ArgumentNullException>(() => new StaticSerializerFactory(null));
        }

        [Fact]
        public void GetDefault()
        {
            var serializer = Substitute.For<ISerializer>();
            var message = Substitute.For<IMessageProperties>();
            message.ContentType.Returns("some-content");

            var factory = new StaticSerializerFactory(serializer, null);

            var result = factory.Get(message);

            Assert.Equal(serializer, result);
        }

        [Fact]
        public void GetByType()
        {
            var serializer = Substitute.For<ISerializer>();
            var message = Substitute.For<IMessageProperties>();
            message.ContentType.Returns("my-content");

            var factory = new StaticSerializerFactory(serializer, "my-content");

            var result = factory.Get(message);

            Assert.Equal(serializer, result);
        }

        [Fact]
        public void ThrowsOnWrongContentType()
        {
            var serializer = Substitute.For<ISerializer>();
            var message = Substitute.For<IMessageProperties>();
            message.ContentType.Returns("other-content");

            var factory = new StaticSerializerFactory(serializer, "my-content");

            Assert.Throws<NotSupportedException>(() => factory.Get(message));
        }
    }
}