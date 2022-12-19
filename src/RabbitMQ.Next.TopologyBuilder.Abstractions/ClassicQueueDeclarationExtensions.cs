using System;

namespace RabbitMQ.Next.TopologyBuilder;

public static class ClassicQueueDeclarationExtensions
{
    public static IClassicQueueDeclaration MessageTtl(this IClassicQueueDeclaration declaration, int expiration)
    {
        if (expiration <= 0)
        {
            throw new ArgumentException(nameof(expiration));
        }

        declaration.Argument("x-message-ttl", expiration);
        return declaration;
    }

    public static IClassicQueueDeclaration AsLazy(this IClassicQueueDeclaration declaration)
    {
        declaration.Argument("queue-mode", "lazy");
        return declaration;
    }
    
    public static IClassicQueueDeclaration DeadLetterExchange(this IClassicQueueDeclaration declaration, string exchange, string deadLetterRoutingKey = null)
    {
        declaration.Argument("x-dead-letter-exchange", exchange);
        if (!string.IsNullOrEmpty(deadLetterRoutingKey))
        {
            declaration.Argument("x-dead-letter-routing-key", deadLetterRoutingKey);
        }
        return declaration;
    }
    
    public static IClassicQueueDeclaration MaxLength(this IClassicQueueDeclaration declaration, int length)
    {
        declaration.Argument("x-max-length", length);
        return declaration;
    }

    public static IClassicQueueDeclaration MaxSize(this IClassicQueueDeclaration declaration, long size)
    {
        declaration.Argument("x-max-length-bytes", size);
        return declaration;
    }
    
    public static IClassicQueueDeclaration OverflowDropHead(this IClassicQueueDeclaration declaration)
    {
        declaration.Argument("x-overflow", "drop-head");
        return declaration;
    }

    public static IClassicQueueDeclaration OverflowReject(this IClassicQueueDeclaration declaration)
    {
        declaration.Argument("x-overflow", "reject-publish");
        return declaration;
    }
    
    public static IClassicQueueDeclaration OverflowDeadLetter(this IClassicQueueDeclaration declaration)
    {
        declaration.Argument("x-overflow", "reject-publish-dlx");
        return declaration;
    }
}