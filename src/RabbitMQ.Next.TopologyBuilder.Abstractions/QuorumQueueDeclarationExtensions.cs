using System;

namespace RabbitMQ.Next.TopologyBuilder;

public static class QuorumQueueDeclarationExtensions
{
    public static IQuorumQueueDeclaration QueueTtl(this IQuorumQueueDeclaration declaration, int expiration)
    {
        if (expiration <= 0)
        {
            throw new ArgumentException(nameof(expiration));
        }

        declaration.Argument("x-expires", expiration);
        return declaration;
    }
    
    public static IQuorumQueueDeclaration MessageTtl(this IQuorumQueueDeclaration declaration, int expiration)
    {
        if (expiration <= 0)
        {
            throw new ArgumentException(nameof(expiration));
        }

        declaration.Argument("x-message-ttl", expiration);
        return declaration;
    }
    
    public static IQuorumQueueDeclaration DeadLetterExchange(this IQuorumQueueDeclaration declaration, string exchange, string deadLetterRoutingKey = null)
    {
        declaration.Argument("x-dead-letter-exchange", exchange);
        if (!string.IsNullOrEmpty(deadLetterRoutingKey))
        {
            declaration.Argument("x-dead-letter-routing-key", deadLetterRoutingKey);
        }
        return declaration;
    }
    
    public static IQuorumQueueDeclaration DeliveryLimit(this IQuorumQueueDeclaration declaration, int deliveryLimit)
    {
        declaration.Argument("x-delivery-limit", deliveryLimit);
        return declaration;
    }
    
    public static IQuorumQueueDeclaration MaxLength(this IQuorumQueueDeclaration declaration, int length)
    {
        declaration.Argument("x-max-length", length);
        return declaration;
    }

    public static IQuorumQueueDeclaration MaxSize(this IQuorumQueueDeclaration declaration, long size)
    {
        declaration.Argument("x-max-length-bytes", size);
        return declaration;
    }
    
    public static IQuorumQueueDeclaration OverflowDropHead(this IQuorumQueueDeclaration declaration)
    {
        declaration.Argument("x-overflow", "drop-head");
        return declaration;
    }

    public static IQuorumQueueDeclaration OverflowReject(this IQuorumQueueDeclaration declaration)
    {
        declaration.Argument("x-overflow", "reject-publish");
        return declaration;
    }

    public static IQuorumQueueDeclaration SingleActiveConsumer(this IQuorumQueueDeclaration declaration)
    {
        declaration.Argument("x-single-active-consumer", true);
        return declaration;
    }
}