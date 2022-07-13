using NSubstitute;
using RabbitMQ.Next.Methods;
using RabbitMQ.Next.Tests.Mocks;
using RabbitMQ.Next.Transport.Methods.Registry;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Registry;

public class MethodRegistrationTests
{
    [Fact]
    public void RegistrationCtor()
    {
        var registration = new MethodRegistration<DummyMethod<int>>(MethodId.Unknown);

        Assert.Equal(MethodId.Unknown, registration.MethodId);
        Assert.Equal(typeof(DummyMethod<int>), registration.Type);
    }

    [Fact]
    public void HasContent()
    {
        var registration = new MethodRegistration<DummyMethod<int>>(MethodId.Unknown);
        ((IMethodRegistrationBuilder<DummyMethod<int>>)registration).HasContent();

        Assert.True(registration.HasContent);
    }

    [Fact]
    public void UseFormatter()
    {
        var formatter = Substitute.For<IMethodFormatter<DummyMethod<int>>>();

        var registration = new MethodRegistration<DummyMethod<int>>(MethodId.Unknown);
        ((IMethodRegistrationBuilder<DummyMethod<int>>)registration).Use(formatter);

        Assert.Equal(formatter, registration.Formatter);
    }

    [Fact]
    public void UseParser()
    {
        var formatter = Substitute.For<IMethodParser<DummyMethod<int>>>();

        var registration = new MethodRegistration<DummyMethod<int>>(MethodId.Unknown);
        ((IMethodRegistrationBuilder<DummyMethod<int>>)registration).Use(formatter);

        Assert.Equal(formatter, registration.Parser);
    }

    [Fact]
    public void IsSync()
    {

    }
}