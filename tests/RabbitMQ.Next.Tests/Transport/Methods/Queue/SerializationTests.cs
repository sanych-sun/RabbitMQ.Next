using System.Collections.Generic;
using RabbitMQ.Next.Transport.Methods;
using RabbitMQ.Next.Transport.Methods.Queue;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Queue;

public class SerializationTests : SerializationTestBase
{
    public SerializationTests()
        : base(builder => builder.AddQueueMethods())
    {}

    [Fact]
    public void DeclareMethodFormatter()
        => this.TestFormatter(new DeclareMethod("my-queue", true, false, false, new Dictionary<string, object> {["a"] = "a"}));

    [Fact]
    public void DeclareOkMethodParser()
        => this.TestParser(new DeclareOkMethod("my-queue", 0, 0));

    [Fact]
    public void BindMethodFormatter()
        => this.TestFormatter(new BindMethod("my-queue", "amq.topic", "test", new Dictionary<string, object> {["b"] = "b"}));

    [Fact]
    public void BindOkMethodParser()
        => this.TestParser(new BindOkMethod());

    [Fact]
    public void PurgeMethodFormatter()
        => this.TestFormatter(new PurgeMethod("my-queue"));

    [Fact]
    public void PurgeOkMethodParser()
        => this.TestParser(new PurgeOkMethod(3));

    [Fact]
    public void UnbindMethodFormatter()
        => this.TestFormatter(new UnbindMethod("my-queue", "amq.topic", "test", new Dictionary<string, object> {["b"] = "b"}));

    [Fact]
    public void UnbindOkMethodParser()
        => this.TestParser(new UnbindOkMethod());

    [Fact]
    public void DeleteMethodFormatter()
        => this.TestFormatter(new DeleteMethod("my-queue", true, false));

    [Fact]
    public void DeleteOkMethodParser()
        => this.TestParser(new DeleteOkMethod(3));
}