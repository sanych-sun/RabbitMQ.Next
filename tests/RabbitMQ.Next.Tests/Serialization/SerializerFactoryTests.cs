using System;
using NSubstitute;
using RabbitMQ.Next.Serialization;
using Xunit;

namespace RabbitMQ.Next.Tests.Serialization;

public class SerializerFactoryTests
{

    [Fact]
    public void CanGetByTypeSerializer()
    {
        var serializer1 = Substitute.For<ISerializer>();
        var serializer2 = Substitute.For<ISerializer>();

        var factory = new SerializerFactory();
        factory.UseSerializer(serializer1, "some-type");
        factory.UseSerializer(serializer2, "other-type");

        var result = factory.Get("some-type");

        Assert.Equal(serializer1, result);
    }

    [Fact]
    public void CanGetDefaultSerializer()
    {
        var serializer1 = Substitute.For<ISerializer>();
        var serializer2 = Substitute.For<ISerializer>();

        var factory = new SerializerFactory();
        factory.UseSerializer(serializer1, "some-type");
        factory.UseSerializer(serializer2, "other-type");
        factory.DefaultSerializer(serializer1);
            

        var result = factory.Get("another-type");

        Assert.Equal(serializer1, result);
    }

    [Fact]
    public void GetByTypeWithDefault()
    {
        var serializer1 = Substitute.For<ISerializer>();
        var serializer2 = Substitute.For<ISerializer>();

        var factory = new SerializerFactory();
        factory.UseSerializer(serializer1, "some-type");
        factory.UseSerializer(serializer2, "other-type");
        factory.DefaultSerializer(serializer2);
            

        var result = factory.Get("some-type");

        Assert.Equal(serializer1, result);
    }

    [Fact]
    public void GetLastDefault()
    {
        var serializer1 = Substitute.For<ISerializer>();
        var serializer2 = Substitute.For<ISerializer>();

        var factory = new SerializerFactory();
        factory.UseSerializer(serializer1, "some-type");
        factory.UseSerializer(serializer2, "other-type");
        factory.DefaultSerializer(serializer2);
        factory.DefaultSerializer(serializer1);
            

        var result = factory.Get("any-type");

        Assert.Equal(serializer1, result);
    }

    [Fact]
    public void ThrowsIfCannotResolve()
    {
        var serializer1 = Substitute.For<ISerializer>();
        var serializer2 = Substitute.For<ISerializer>();
            
        var factory = new SerializerFactory();
        factory.UseSerializer(serializer1, "some-type");
        factory.UseSerializer(serializer2, "other-type");

        Assert.Throws<NotSupportedException>(() => factory.Get("not-supported"));
    }
}