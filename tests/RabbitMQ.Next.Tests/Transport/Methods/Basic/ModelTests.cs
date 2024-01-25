using System.Collections.Generic;
using RabbitMQ.Next.Transport.Methods.Basic;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Basic;

public class ModelTests
{
    [Fact]
    public void QosMethod()
    {
        const uint prefetchSize = 12345;
        const ushort prefetchCount = 321;

        var method = new QosMethod(prefetchSize, prefetchCount, true);

        Assert.Equal(MethodId.BasicQos, method.MethodId);
        Assert.Equal(prefetchSize, method.PrefetchSize);
        Assert.Equal(prefetchCount, method.PrefetchCount);
        Assert.True(method.Global);
    }

    [Fact]
    public void QosOkMethod()
    {
        var method = new QosOkMethod();

        Assert.Equal(MethodId.BasicQosOk, method.MethodId);
    }

    [Fact]
    public void ConsumeMethod()
    {
        const string queue = "my-queue";
        const string consumerTag = "tag";
        const byte flags = 0;
        var args = new Dictionary<string, object>();

        var method = new ConsumeMethod(queue, consumerTag, flags, args);

        Assert.Equal(MethodId.BasicConsume, method.MethodId);
        Assert.Equal(queue, method.Queue);
        Assert.Equal(consumerTag, method.ConsumerTag);
        Assert.Equal(flags, method.Flags);
        Assert.Equal(args, method.Arguments);
    }

    [Theory]
    [InlineData(0b_00000000, false, false, false)]
    [InlineData(0b_00000001, true, false, false)]
    [InlineData(0b_00000010, false, true, false)]
    [InlineData(0b_00000100, false, false, true)]
    [InlineData(0b_00000111, true, true, true)]
    public void ConsumeMethodFlags(byte expected, bool noLocal, bool noAck, bool exclusive)
    {
        var method = new ConsumeMethod("queue", "tag", noLocal, noAck, exclusive, null);

        Assert.Equal(expected, method.Flags);
    }

    [Fact]
    public void ConsumeOkMethod()
    {
        const string consumerTag = "tag";

        var method = new ConsumeOkMethod(consumerTag);

        Assert.Equal(MethodId.BasicConsumeOk, method.MethodId);
        Assert.Equal(consumerTag, method.ConsumerTag);
    }

    [Fact]
    public void CancelMethod()
    {
        const string consumerTag = "tag";

        var method = new CancelMethod(consumerTag);

        Assert.Equal(MethodId.BasicCancel, method.MethodId);
        Assert.Equal(consumerTag, method.ConsumerTag);
    }

    [Fact]
    public void CancelOkMethod()
    {
        const string consumerTag = "tag";

        var method = new CancelOkMethod(consumerTag);

        Assert.Equal(MethodId.BasicCancelOk, method.MethodId);
        Assert.Equal(consumerTag, method.ConsumerTag);
    }

    [Fact]
    public void PublishMethod()
    {
        const string exchange = "exchange";
        const string routingKey = "routing";
        const byte flags = 0b_00000001;

        var method = new PublishMethod(exchange, routingKey, flags);

        Assert.Equal(MethodId.BasicPublish, method.MethodId);
        Assert.Equal(exchange, method.Exchange);
        Assert.Equal(routingKey, method.RoutingKey);
        Assert.Equal(flags, method.Flags);
    }

    [Theory]
    [InlineData(0b_00000000, false, false)]
    [InlineData(0b_00000001, true, false)]
    [InlineData(0b_00000010, false, true)]
    [InlineData(0b_00000011, true, true)]
    public void PublishMethodFlags(byte expected, bool mandatory, bool immediate)
    {
        var method = new PublishMethod("exchange", "routing", mandatory, immediate);

        Assert.Equal(expected, method.Flags);
    }

    [Fact]
    public void ReturnMethod()
    {
        const string exchange = "exchange";
        const string routingKey = "routing";
        const ushort replyCode = 400;
        const string replyText = "some error";

        var method = new ReturnMethod(exchange, routingKey, replyCode, replyText);

        Assert.Equal(MethodId.BasicReturn, method.MethodId);
        Assert.Equal(exchange, method.Exchange);
        Assert.Equal(routingKey, method.RoutingKey);
        Assert.Equal(replyCode, method.ReplyCode);
        Assert.Equal(replyText, method.ReplyText);
    }

    [Fact]
    public void DeliverMethod()
    {
        const string exchange = "exchange";
        const string routingKey = "routing";
        const string consumerTag = "tag";
        const ulong deliveryTag = 42;

        var method = new DeliverMethod(exchange, routingKey, consumerTag, deliveryTag, true);

        Assert.Equal(MethodId.BasicDeliver, method.MethodId);
        Assert.Equal(exchange, method.Exchange);
        Assert.Equal(routingKey, method.RoutingKey);
        Assert.Equal(consumerTag, method.ConsumerTag);
        Assert.Equal(deliveryTag, method.DeliveryTag);
        Assert.True(method.Redelivered);
    }

    [Fact]
    public void GetMethod()
    {
        const string queue = "queue";

        var method = new GetMethod(queue, true);

        Assert.Equal(MethodId.BasicGet, method.MethodId);
        Assert.Equal(queue, method.Queue);
        Assert.True(method.NoAck);
    }

    [Fact]
    public void GetOkMethod()
    {
        const string exchange = "exchange";
        const string routingKey = "routing";
        const ulong deliveryTag = 42;
        const uint messageCount = 35;

        var method = new GetOkMethod(exchange, routingKey, deliveryTag, true, messageCount);

        Assert.Equal(MethodId.BasicGetOk, method.MethodId);
        Assert.Equal(exchange, method.Exchange);
        Assert.Equal(routingKey, method.RoutingKey);
        Assert.Equal(deliveryTag, method.DeliveryTag);
        Assert.True(method.Redelivered);
        Assert.Equal(messageCount, method.MessageCount);
    }

    [Fact]
    public void GetEmptyMethod()
    {
        var method = new GetEmptyMethod();

        Assert.Equal(MethodId.BasicGetEmpty, method.MethodId);
    }

    [Fact]
    public void AckMethod()
    {
        const ulong deliveryTag = 42;

        var method = new AckMethod(deliveryTag, true);

        Assert.Equal(MethodId.BasicAck, method.MethodId);
        Assert.Equal(deliveryTag, method.DeliveryTag);
        Assert.True(method.Multiple);
    }

    [Fact]
    public void RecoverMethod()
    {
        var method = new RecoverMethod(true);

        Assert.Equal(MethodId.BasicRecover, method.MethodId);
        Assert.True(method.Requeue);
    }

    [Fact]
    public void RecoverOkMethod()
    {
        var method = new RecoverOkMethod();

        Assert.Equal(MethodId.BasicRecoverOk, method.MethodId);
    }

    [Fact]
    public void NackMethod()
    {
        const ulong deliveryTag = 42;

        var method = new NackMethod(deliveryTag, true, false);

        Assert.Equal(MethodId.BasicNack, method.MethodId);
        Assert.Equal(deliveryTag, method.DeliveryTag);
        Assert.True(method.Multiple);
        Assert.False(method.Requeue);
    }
}
