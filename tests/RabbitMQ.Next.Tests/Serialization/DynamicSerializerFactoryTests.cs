using System;
using NSubstitute;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Serialization.Abstractions;
using Xunit;

namespace RabbitMQ.Next.Tests.Serialization
{
    public class DynamicSerializerFactoryTests
    {

        [Fact]
        public void ThrowsOnNullSerializers()
        {
            Assert.Throws<ArgumentNullException>(() => new DynamicSerializerFactory(null));
        }

        [Fact]
        public void ThrowsOnEmptySerializers()
        {
            Assert.Throws<ArgumentNullException>(() => new DynamicSerializerFactory(Array.Empty<(ISerializer,string,bool)>()));
        }

        [Fact]
        public void CanGetByTypeSerializer()
        {
            var serializer1 = Substitute.For<ISerializer>();
            var serializer2 = Substitute.For<ISerializer>();
            var message = Substitute.For<IMessageProperties>();
            message.ContentType.Returns("some-type");

            var factory = new DynamicSerializerFactory(new[]
            {
                (serializer1, "some-type", false),
                (serializer2, "other-type", false)
            });

            var result = factory.Get(message);

            Assert.Equal(serializer1, result);
        }

        [Fact]
        public void CanGetDefaultSerializer()
        {
            var serializer1 = Substitute.For<ISerializer>();
            var serializer2 = Substitute.For<ISerializer>();
            var message = Substitute.For<IMessageProperties>();
            message.ContentType.Returns("another-type");

            var factory = new DynamicSerializerFactory(new[]
            {
                (serializer1, "some-type", true),
                (serializer2, "other-type", false)
            });

            var result = factory.Get(message);

            Assert.Equal(serializer1, result);
        }

        [Fact]
        public void GetByTypeWithDefault()
        {
            var serializer1 = Substitute.For<ISerializer>();
            var serializer2 = Substitute.For<ISerializer>();
            var message = Substitute.For<IMessageProperties>();
            message.ContentType.Returns("other-type");

            var factory = new DynamicSerializerFactory(new[]
            {
                (serializer1, "some-type", true),
                (serializer2, "other-type", false)
            });

            var result = factory.Get(message);

            Assert.Equal(serializer2, result);
        }

        [Fact]
        public void GetLastDefault()
        {
            var serializer1 = Substitute.For<ISerializer>();
            var serializer2 = Substitute.For<ISerializer>();
            var message = Substitute.For<IMessageProperties>();
            message.ContentType.Returns("another-type");

            var factory = new DynamicSerializerFactory(new[]
            {
                (serializer1, "some-type", true),
                (serializer2, "other-type", true)
            });

            var result = factory.Get(message);

            Assert.Equal(serializer2, result);
        }

        [Fact]
        public void ThrowsIfCannotResolve()
        {
            var serializer1 = Substitute.For<ISerializer>();
            var serializer2 = Substitute.For<ISerializer>();
            var message = Substitute.For<IMessageProperties>();
            message.ContentType.Returns("another-type");

            var factory = new DynamicSerializerFactory(new[]
            {
                (serializer1, "some-type", false),
                (serializer2, "other-type", false)
            });

            Assert.Throws<NotSupportedException>(() => factory.Get(message));
        }
    }
}