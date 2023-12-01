using System;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Auth;
using Xunit;

namespace RabbitMQ.Next.Tests.Abstractions.Auth;

public class PlainAuthMechanismTests
{
    [Fact]
    public void CtorTests()
    {
        var user = "test";
        var password = "pwd";

        var auth = new PlainAuthMechanism(user, password);

        Assert.Equal("PLAIN", auth.Type);
        Assert.Equal(user, auth.UserName);
        Assert.Equal(password, auth.Password);
    }

    [Fact]
    public async Task HandleChallengeAsync()
    {
        var user = "test";
        var password = "pwd";
        var expected = "\0test\0pwd"u8.ToArray();

        var auth = new PlainAuthMechanism(user, password);
        var response = await auth.HandleChallengeAsync(ReadOnlySpan<byte>.Empty);
        
        Assert.Equal(expected, response.ToArray());
    }

    [Fact]
    public void ExtensionTests()
    {
        var user = "test2";
        var password = "pwd2";

        var builder = Substitute.For<IConnectionBuilder>();
        builder.PlainAuth(user, password);

        builder.Received().Auth(Arg.Is<IAuthMechanism>(a => ((PlainAuthMechanism)a).UserName == user && ((PlainAuthMechanism)a).Password == password)
        );
    }
}