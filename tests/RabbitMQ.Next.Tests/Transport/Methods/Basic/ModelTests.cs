using System.Collections.Generic;
using RabbitMQ.Next.Transport.Methods.Basic;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Basic;

public class ModelTests
{
    [Fact]
    public void QosMethod()
    {
        var prefetchSize = (uint)12345;
        var prefetchCount = (ushort)321;
        var global = true;

        var method = new QosMethod(prefetchSize, prefetchCount, global);

        Assert.Equal(MethodId.BasicQos, method.MethodId);
        Assert.Equal(prefetchSize, method.PrefetchSize);
        Assert.Equal(prefetchCount, method.PrefetchCount);
        Assert.Equal(global, method.Global);
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
        var queue = "my-queue";
        var consumerTag = "tag";
        var noLocal = true;
        var noAck = true;
        var exclusive = true;
        var args = new Dictionary<string, object>();

        var method = new ConsumeMethod(queue, consumerTag, noLocal, noAck, exclusive, args);

        Assert.Equal(MethodId.BasicConsume, method.MethodId);
        Assert.Equal(queue, method.Queue);
        Assert.Equal(consumerTag, method.ConsumerTag);
        Assert.Equal(noLocal, method.NoLocal);
        Assert.Equal(noAck, method.NoAck);
        Assert.Equal(exclusive, method.Exclusive);
        Assert.Equal(args, method.Arguments);
    }

    [Fact]
    public void ConsumeOkMethod()
    {
        var consumerTag = "tag";

        var method = new ConsumeOkMethod(consumerTag);

        Assert.Equal(MethodId.BasicConsumeOk, method.MethodId);
        Assert.Equal(consumerTag, method.ConsumerTag);
    }

    [Fact]
    public void CancelMethod()
    {
        var consumerTag = "tag";

        var method = new CancelMethod(consumerTag);

        Assert.Equal(MethodId.BasicCancel, method.MethodId);
        Assert.Equal(consumerTag, method.ConsumerTag);
    }

    [Fact]
    public void CancelOkMethod()
    {
        var consumerTag = "tag";

        var method = new CancelOkMethod(consumerTag);

        Assert.Equal(MethodId.BasicCancelOk, method.MethodId);
        Assert.Equal(consumerTag, method.ConsumerTag);
    }

    [Fact]
    public void PublishMethod()
    {
        var exchange = "exchange";
        var routingKey = "routing";
        var flags = (byte)0b_00000001;
        var mandatory = true;
        var immediate = true;

        var method = new PublishMethod(exchange, routingKey, mandatory, immediate);

        Assert.Equal(MethodId.BasicPublish, method.MethodId);
        Assert.Equal(exchange, method.Exchange);
        Assert.Equal(routingKey, method.RoutingKey);
        Assert.Equal(mandatory, method.Mandatory);
        Assert.Equal(immediate, method.Immediate);
    }

    [Fact]
    public void ReturnMethod()
    {
        var exchange = "exchange";
        var routingKey = "routing";
        var replyCode = (ushort)400;
        var replyText = "some error";

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
        var exchange = "exchange";
        var routingKey = "routing";
        var consumerTag = "tag";
        var deliveryTag = (ulong)42;
        var redelivered = true;

        var method = new DeliverMethod(exchange, routingKey, consumerTag, deliveryTag, redelivered);

        Assert.Equal(MethodId.BasicDeliver, method.MethodId);
        Assert.Equal(exchange, method.Exchange);
        Assert.Equal(routingKey, method.RoutingKey);
        Assert.Equal(consumerTag, method.ConsumerTag);
        Assert.Equal(deliveryTag, method.DeliveryTag);
        Assert.Equal(redelivered, method.Redelivered);
    }

    [Fact]
    public void GetMethod()
    {
        var queue = "queue";
        var noAck = true;

        var method = new GetMethod(queue, noAck);

        Assert.Equal(MethodId.BasicGet, method.MethodId);
        Assert.Equal(queue, method.Queue);
        Assert.Equal(noAck, method.NoAck);
    }

    [Fact]
    public void GetOkMethod()
    {
        var exchange = "exchange";
        var routingKey = "routing";
        var deliveryTag = (ulong)42;
        var redelivered = true;
        var messageCount = (uint)35;

        var method = new GetOkMethod(exchange, routingKey, deliveryTag, redelivered, messageCount);

        Assert.Equal(MethodId.BasicGetOk, method.MethodId);
        Assert.Equal(exchange, method.Exchange);
        Assert.Equal(routingKey, method.RoutingKey);
        Assert.Equal(deliveryTag, method.DeliveryTag);
        Assert.Equal(redelivered, method.Redelivered);
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
        var deliveryTag = (ulong)42;
        var multiple = true;

        var method = new AckMethod(deliveryTag, multiple);

        Assert.Equal(MethodId.BasicAck, method.MethodId);
        Assert.Equal(deliveryTag, method.DeliveryTag);
        Assert.Equal(multiple, method.Multiple);
    }

    [Fact]
    public void RecoverMethod()
    {
        var requeue = true;

        var method = new RecoverMethod(requeue);

        Assert.Equal(MethodId.BasicRecover, method.MethodId);
        Assert.Equal(requeue, method.Requeue);
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
        var deliveryTag = (ulong)42;
        var multiple = false;
        var requeue = true;

        var method = new NackMethod(deliveryTag, multiple, requeue);

        Assert.Equal(MethodId.BasicNack, method.MethodId);
        Assert.Equal(deliveryTag, method.DeliveryTag);
        Assert.Equal(multiple, method.Multiple);
        Assert.Equal(requeue, method.Requeue);
    }
}