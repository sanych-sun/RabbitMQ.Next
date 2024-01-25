using System;
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
        const string user = "test";
        const string password = "pwd";

        var auth = new PlainAuthMechanism(user, password);

        Assert.Equal("PLAIN", auth.Type);
        Assert.Equal(user, auth.UserName);
        Assert.Equal(password, auth.Password);
    }

    [Fact]
    public async Task StartAsyncTests()
    {
        const string user = "test";
        const string password = "pwd";
        var expected = "\0test\0pwd"u8.ToArray();

        var auth = new PlainAuthMechanism(user, password);
        var response = await auth.StartAsync();
        
        Assert.Equal(expected, response.ToArray());
    }
    
    [Fact]
    public async Task HandleChallengeAsyncThrows()
    {
        var auth = new PlainAuthMechanism("test", "pwd");

        await Assert.ThrowsAsync<NotSupportedException>(async () => await auth.HandleChallengeAsync(ReadOnlySpan<byte>.Empty));
    }

    [Fact]
    public void ExtensionTests()
    {
        const string user = "test2";
        const string password = "pwd2";

        var builder = Substitute.For<IConnectionBuilder>();
        builder.PlainAuth(user, password);

        builder.Received().Auth(Arg.Is<IAuthMechanism>(a => ((PlainAuthMechanism)a).UserName == user && ((PlainAuthMechanism)a).Password == password)
        );
    }
}
