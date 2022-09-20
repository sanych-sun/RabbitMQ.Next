using RabbitMQ.Next.Transport.Methods.Channel;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Channel;

public class SerializationTests : SerializationTestBase
{
    [Fact]
    public void OpenMethodFormatter()
        => this.TestFormatter(new OpenMethod());

    [Fact]
    public void OpenOkMethodParser()
        => this.TestParser(new OpenOkMethod());

    [Fact]
    public void FlowMethodFormatter()
        => this.TestFormatter(new FlowMethod(true));

    [Fact]
    public void FlowMethodParser()
        => this.TestParser(new FlowMethod(true));

    [Fact]
    public void FlowOkMethodFormatter()
        => this.TestFormatter(new FlowOkMethod(true));

    [Fact]
    public void FlowOkMethodParser()
        => this.TestParser(new FlowOkMethod(true));

    [Fact]
    public void CloseMethodFormatter()
        => this.TestFormatter(new CloseMethod(200, "Goodbye", 0));

    [Fact]
    public void CloseMethodParser()
        => this.TestParser(new CloseMethod(200, "Goodbye", 0));

    [Fact]
    public void CloseOkMethodFormatter()
        => this.TestFormatter(new CloseOkMethod());

    [Fact]
    public void CloseOkMethodParser()
        => this.TestParser(new CloseOkMethod());
}