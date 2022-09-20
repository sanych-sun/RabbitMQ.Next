using System;
using NSubstitute;
using RabbitMQ.Next.Tests.Mocks;
using RabbitMQ.Next.Transport.Methods;
using RabbitMQ.Next.Transport.Methods.Registry;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Registry;

public class MethodRegistryTests
{
    private readonly IMethodFormatter<DummyMethod<int>> formatter;
    private readonly IMethodParser<DummyMethod<string>> parser;
    private readonly IMethodRegistry registry;

    public MethodRegistryTests()
    {
        this.formatter = Substitute.For<IMethodFormatter<DummyMethod<int>>>();
        this.parser = Substitute.For<IMethodParser<DummyMethod<string>>>();

        this.registry = new MethodRegistryBuilder()
            .Register<DummyMethod<int>>(MethodId.BasicGet,
                registration => registration
                    .HasContent()
                    .Use(this.formatter)
            )
            .Register<DummyMethod<string>>(MethodId.BasicAck,
                registration => registration
                    .Use(this.parser)
            ).Build();
    }

    [Theory]
    [InlineData(true, MethodId.BasicGet)]
    [InlineData(false, MethodId.BasicAck)]
    public void HasContentTests(bool expected, MethodId methodId)
    {
        var result = this.registry.HasContent(methodId);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(typeof(DummyMethod<int>), MethodId.BasicGet)]
    [InlineData(typeof(DummyMethod<string>), MethodId.BasicAck)]
    public void GetMethodType(Type expected, MethodId methodId)
    {
        var result = this.registry.GetMethodType(methodId);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetMethodId1()
    {
        var result = this.registry.GetMethodId<DummyMethod<int>>();

        Assert.Equal(MethodId.BasicGet, result);
    }

    [Fact]
    public void GetMethodId2()
    {
        var result = this.registry.GetMethodId<DummyMethod<string>>();

        Assert.Equal(MethodId.BasicAck, result);
    }

    [Fact]
    public void GetParserByType()
    {
        var result = this.registry.GetParser<DummyMethod<string>>();

        Assert.Equal(this.parser, result);
    }

    [Fact]
    public void GetFormatter()
    {
        var result = this.registry.GetFormatter<DummyMethod<int>>();

        Assert.Equal(this.formatter, result);
    }

    [Fact]
    public void GetParserByTypeReturnsNUllIfNoParser()
    {
        var result = this.registry.GetParser<DummyMethod<int>>();
        Assert.Null(result);
    }

    [Fact]
    public void GetFormatterReturnsNullIfNoFormatter()
    {
        var result = this.registry.GetFormatter<DummyMethod<string>>();
        Assert.Null(result);
    }

    [Fact]
    public void HasContentThrowsIfNotRegistered()
    {
        Assert.Throws<NotSupportedException>(() => this.registry.HasContent(MethodId.Unknown));
    }

    [Fact]
    public void GetMethodTypeThrowsIfNotRegistered()
    {
        Assert.Throws<NotSupportedException>(() => this.registry.GetMethodType(MethodId.Unknown));
    }

    [Fact]
    public void GetMethodIdThrowsIfNotRegistered()
    {
        Assert.Throws<NotSupportedException>(() => this.registry.GetMethodId<DummyMethod<long>>());
    }


    [Fact]
    public void GetParserByTypeThrowsIfNotRegistered()
    {
        Assert.Throws<NotSupportedException>(() => this.registry.GetParser<DummyMethod<long>>());
    }

    [Fact]
    public void GetFormatterThrowsIfNotRegistered()
    {
        Assert.Throws<NotSupportedException>(() => this.registry.GetFormatter<DummyMethod<long>>());
    }

    [Fact]
    public void ThrowsOnEmptyItems()
    {
        Assert.Throws<ArgumentNullException>(() => new MethodRegistry(null));
    }
}