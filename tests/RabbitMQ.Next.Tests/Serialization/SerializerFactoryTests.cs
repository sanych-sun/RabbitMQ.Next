using System;
using NSubstitute;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Serialization;
using Xunit;

namespace RabbitMQ.Next.Tests.Serialization
{
    public class SerializerFactoryTests
    {
        [Fact]
        public void ThrowsOnNullSerializers()
        {
            Assert.Throws<ArgumentNullException>(() => SerializerFactory.Create(null));
        }

        [Fact]
        public void ThrowsOnEmptySerializers()
        {
            Assert.Throws<ArgumentNullException>(() => SerializerFactory.Create(Array.Empty<(ISerializer,string,bool)>()));
        }

        [Fact]
        public void CreateStaticFactoryForSingle()
        {
            var serializer = Substitute.For<ISerializer>();

            var result = SerializerFactory.Create(new[] {(serializer, "some-type", false)});

            Assert.IsType<StaticSerializerFactory>(result);
        }

        [Fact]
        public void CreateDynamicFactoryForMultiple()
        {
            var serializer1 = Substitute.For<ISerializer>();
            var serializer2 = Substitute.For<ISerializer>();

            var result = SerializerFactory.Create(new[]
            {
                (serializer1, "some-type", false),
                (serializer2, "other-type", false)
            });

            Assert.IsType<DynamicSerializerFactory>(result);
        }
    }
}