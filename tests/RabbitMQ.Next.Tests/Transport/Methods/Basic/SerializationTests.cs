using System.Collections.Generic;
using RabbitMQ.Next.Transport.Methods.Basic;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Basic;

public class SerializationTests : SerializationTestBase
{
    [Fact]
    public void QosMethodFormatter()
        => this.TestFormatter(new QosMethod(54321, 25, false));

    [Fact]
    public void QosOkMethodParser()
        => this.TestParser(new QosOkMethod());

    [Fact]
    public void ConsumeMethodFormatter()
        => this.TestFormatter(new ConsumeMethod("my-queue", "ctag", false, false, false, new Dictionary<string, object>()
        {
            ["x-priority"] = 2
        }));

    [Fact]
    public void ConsumeOkMethodParser()
        => this.TestParser(new ConsumeOkMethod("ctag"));

    [Fact]
    public void CancelMethodFormatter()
        => this.TestFormatter(new CancelMethod("ctag"));

    [Fact]
    public void CancelOkMethodParser()
        => this.TestParser(new CancelOkMethod("ctag"));


    [Fact]
    public void PublishMethodFormatter()
        => this.TestFormatter(new PublishMethod("exchange", "routingKey", false, false));

    [Fact]
    public void ReturnMethodParser()
        => this.TestParser(new ReturnMethod("exchange", "routingKey", 400, "some error"));

    [Fact]
    public void DeliverMethodParser()
        => this.TestParser(new DeliverMethod("exchange", "routingKey", "consumer", 42, true));

    [Fact]
    public void GetMethodFormatter()
        => this.TestFormatter(new GetMethod("my-queue", false));

    [Fact]
    public void GetOkMethodParser()
        => this.TestParser(new GetOkMethod("exchange", "routingKey", 42, true, 321));

    [Fact]
    public void GetEmptyMethodParser()
        => this.TestParser(new GetEmptyMethod());

    [Fact]
    public void AckMethodFormatter()
        => this.TestFormatter(new AckMethod(24, true));

    [Fact]
    public void AckMethodParser()
        => this.TestParser(new AckMethod(24, true));

    [Fact]
    public void RecoverMethodFormatter()
        => this.TestFormatter(new RecoverMethod(true));

    [Fact]
    public void RecoverOkMethodParser()
        => this.TestParser(new RecoverOkMethod());

    [Fact]
    public void NackMethodFormatter()
        => this.TestFormatter(new NackMethod(24, true, true));

    [Fact]
    public void NackMethodParser()
        => this.TestParser(new NackMethod(24, true, true));
}