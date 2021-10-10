using System;
using NSubstitute;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Serialization.Abstractions;
using Xunit;

namespace RabbitMQ.Next.Tests.Serialization
{
    public class SerializerFactoryTests
    {
        [Fact]
        public void RegisterSerializer()
        {
            var serializerFactory = new SerializerFactory();
            var serializer1 = Substitute.For<ISerializer>();
            var serializer2 = Substitute.For<ISerializer>();

            serializerFactory.RegisterSerializer(serializer1, new[] {"test1"});
            serializerFactory.RegisterSerializer(serializer2, new[] {"test2"});

            Assert.Equal(serializer1, ((ISerializerFactory)serializerFactory).Get("test1"));
            Assert.Equal(serializer2, ((ISerializerFactory)serializerFactory).Get("test2"));
        }
        
        [Fact]
        public void RegisterSerializerAsDefault()
        {
            var serializerFactory = new SerializerFactory();
            var serializer1 = Substitute.For<ISerializer>();

            serializerFactory.RegisterSerializer(serializer1);

            Assert.Equal(serializer1, ((ISerializerFactory)serializerFactory).Get(string.Empty));
        }

        [Fact]
        public void RegisterSerializerCanOverride()
        {
            var serializerFactory = new SerializerFactory();
            var serializer1 = Substitute.For<ISerializer>();
            var serializer2 = Substitute.For<ISerializer>();

            serializerFactory.RegisterSerializer(serializer1, new[] {"test1"}, false);
            serializerFactory.RegisterSerializer(serializer2, new[] {"test1"}, false);

            Assert.Equal(serializer2, ((ISerializerFactory)serializerFactory).Get("test1"));
        }

        [Fact]
        public void RegisterSerializerCanOverrideDefault()
        {
            var serializerFactory = new SerializerFactory();
            var serializer1 = Substitute.For<ISerializer>();
            var serializer2 = Substitute.For<ISerializer>();

            serializerFactory.RegisterSerializer(serializer1);
            serializerFactory.RegisterSerializer(serializer2);

            Assert.Equal(serializer2, ((ISerializerFactory)serializerFactory).Get(string.Empty));
        }

        [Fact]
        public void NonDefaultRegisterDoesNotOverrideDefault()
        {
            var serializerFactory = new SerializerFactory();
            var serializer1 = Substitute.For<ISerializer>();
            var serializer2 = Substitute.For<ISerializer>();

            serializerFactory.RegisterSerializer(serializer1);
            serializerFactory.RegisterSerializer(serializer2, new []{"test"}, false);

            Assert.Equal(serializer1, ((ISerializerFactory)serializerFactory).Get(string.Empty));
        }

        [Fact]
        public void ReturnsDefaultForUnknownContentType()
        {
            var serializerFactory = new SerializerFactory();
            var serializer1 = Substitute.For<ISerializer>();

            serializerFactory.RegisterSerializer(serializer1, new[] {"test1"}, true);

            Assert.Equal(serializer1, ((ISerializerFactory)serializerFactory).Get("unknown-type"));
        }

        [Fact]
        public void RegisterSerializerThrowsOnNull()
        {
            var serializerFactory = new SerializerFactory();

            Assert.Throws<ArgumentNullException>(() => serializerFactory.RegisterSerializer(null));
        }
    }
}