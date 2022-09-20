using RabbitMQ.Next.Transport.Methods.Confirm;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Confirm;

public class SerializationTests: SerializationTestBase
{
    [Fact]
    public void SelectMethodFormatter()
        => this.TestFormatter(new SelectMethod());

    [Fact]
    public void SelectOkMethodParser()
        => this.TestParser(new SelectOkMethod());
}