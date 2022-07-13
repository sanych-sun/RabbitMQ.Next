using System.Collections.Generic;
using RabbitMQ.Next.Transport.Methods.Queue;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Queue;

public class ModelTests
{
    [Theory]
    [MemberData(nameof(DeclareTestCases))]
    public void DeclareMethod(string name, bool durable, bool exclusive, bool autoDelete, IReadOnlyDictionary<string, object> arguments)
    {
        var method = new DeclareMethod(name, durable, exclusive, autoDelete, arguments);

        Assert.Equal(MethodId.QueueDeclare, method.MethodId);
        Assert.Equal(name, method.Queue);
        Assert.False(method.Passive);
        Assert.Equal(durable, method.Durable);
        Assert.Equal(exclusive, method.Exclusive);
        Assert.Equal(autoDelete, method.AutoDelete);
        Assert.Equal(arguments, method.Arguments);
    }

    public static IEnumerable<object[]> DeclareTestCases()
    {
        yield return new object[] {"queue", false, false, false, null};
        yield return new object[] { "queue", true, false, false, null};
        yield return new object[] { "queue", false, true, false, null};
        yield return new object[] { "queue", false, false, true, null};
        yield return new object[] { "queue", true, true, true, null};
        yield return new object[] { "queue", true, true, true, new Dictionary<string, object>()
        {
            ["a"] = "a",
        }};
    }

    [Fact]
    public void DeclarePassiveMethod()
    {
        var name = "queue";

        var method = new DeclareMethod(name);

        Assert.Equal(MethodId.QueueDeclare, method.MethodId);
        Assert.Equal(name, method.Queue);
        Assert.True(method.Passive);
        Assert.False(method.Durable);
        Assert.False(method.AutoDelete);
        Assert.False(method.Exclusive);
        Assert.Null(method.Arguments);
    }

    [Fact]
    public void DeclareOkMethod()
    {
        var name = "queueName";
        uint messageCount = 10;
        uint consumerCount = 20;

        var method = new DeclareOkMethod(name, messageCount, consumerCount);

        Assert.Equal(MethodId.QueueDeclareOk, method.MethodId);
        Assert.Equal(name, method.Queue);
        Assert.Equal(messageCount, method.MessageCount);
        Assert.Equal(consumerCount, method.ConsumerCount);
    }

    [Fact]
    public void BindMethod()
    {
        var queue = "destination";
        var exchange = "source";
        var routingKey = "routingKey";
        var arguments = new Dictionary<string, object>()
        {
            ["a"] = "a",
        };

        var method = new BindMethod(queue, exchange, routingKey, arguments);

        Assert.Equal(MethodId.QueueBind, method.MethodId);
        Assert.Equal(queue, method.Queue);
        Assert.Equal(exchange, method.Exchange);
        Assert.Equal(routingKey, method.RoutingKey);
        Assert.Equal(arguments, method.Arguments);
    }

    [Fact]
    public void BindOkMethod()
    {
        var method = new BindOkMethod();

        Assert.Equal(MethodId.QueueBindOk, method.MethodId);
    }

    [Fact]
    public void UnbindMethod()
    {
        var queue = "destination";
        var exchange = "source";
        var routingKey = "routingKey";
        var arguments = new Dictionary<string, object>()
        {
            ["a"] = "a",
        };

        var method = new UnbindMethod(queue, exchange, routingKey, arguments);

        Assert.Equal(MethodId.QueueUnbind, method.MethodId);
        Assert.Equal(queue, method.Queue);
        Assert.Equal(exchange, method.Exchange);
        Assert.Equal(routingKey, method.RoutingKey);
        Assert.Equal(arguments, method.Arguments);
    }

    [Fact]
    public void UnbindOkMethod()
    {
        var method = new UnbindOkMethod();

        Assert.Equal(MethodId.QueueUnbindOk, method.MethodId);
    }

    [Fact]
    public void PurgeMethod()
    {
        var queue = "destination";

        var method = new PurgeMethod(queue);

        Assert.Equal(MethodId.QueuePurge, method.MethodId);
        Assert.Equal(queue, method.Queue);
    }

    [Fact]
    public void PurgeOkMethod()
    {
        uint messageCount = 10;

        var method = new PurgeOkMethod(messageCount);

        Assert.Equal(MethodId.QueuePurgeOk, method.MethodId);
        Assert.Equal(messageCount, method.MessageCount);
    }
        
    [Fact]
    public void DeleteMethod()
    {
        var queue = "destination";
        var unusedOnly = true;
        var emptyOnly = true;

        var method = new DeleteMethod(queue, unusedOnly, emptyOnly);

        Assert.Equal(MethodId.QueueDelete, method.MethodId);
        Assert.Equal(queue, method.Queue);
        Assert.Equal(unusedOnly, method.UnusedOnly);
        Assert.Equal(emptyOnly, method.EmptyOnly);
    }

    [Fact]
    public void DeleteOkMethod()
    {
        uint messageCount = 10;

        var method = new DeleteOkMethod(messageCount);

        Assert.Equal(MethodId.QueueDeleteOk, method.MethodId);
        Assert.Equal(messageCount, method.MessageCount);
    }
}