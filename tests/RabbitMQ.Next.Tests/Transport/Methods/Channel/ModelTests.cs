using RabbitMQ.Next.Transport.Methods.Channel;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Channel;

public class ModelTests
{
    [Fact]
    public void OpenMethod()
    {
        var method = new OpenMethod();

        Assert.Equal(MethodId.ChannelOpen, method.MethodId);
    }

    [Fact]
    public void OpenOkMethod()
    {
        var method = new OpenOkMethod();

        Assert.Equal(MethodId.ChannelOpenOk, method.MethodId);
    }

    [Fact]
    public void FlowMethod()
    {
        var method = new FlowMethod(true);

        Assert.Equal(MethodId.ChannelFlow, method.MethodId);
        Assert.True(method.Active);
    }

    [Fact]
    public void FlowOkMethod()
    {
        var method = new FlowOkMethod(true);

        Assert.Equal(MethodId.ChannelFlowOk, method.MethodId);
        Assert.True(method.Active);
    }

    [Fact]
    public void CloseMethod()
    {
        var method = new CloseMethod(504, "SomeError", MethodId.BasicDeliver);

        Assert.Equal(MethodId.ChannelClose, method.MethodId);
        Assert.Equal(504, method.StatusCode);
        Assert.Equal("SomeError", method.Description);
        Assert.Equal(MethodId.BasicDeliver, method.FailedMethodId);
    }

    [Fact]
    public void CloseOkMethod()
    {
        var method = new CloseOkMethod();

        Assert.Equal(MethodId.ChannelCloseOk, method.MethodId);
    }
}