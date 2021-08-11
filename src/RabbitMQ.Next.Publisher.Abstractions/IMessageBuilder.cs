using System;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Publisher.Abstractions
{
    public interface IMessageBuilder
    {
        IMessageBuilder RoutingKey(string routingKey);
        IMessageBuilder ContentType(string contentType);
        IMessageBuilder ContentEncoding(string contentEncoding);
        IMessageBuilder SetHeader(string key, object value);
        IMessageBuilder DeliveryMode(DeliveryMode deliveryMode);
        IMessageBuilder Priority(byte? priority);
        IMessageBuilder CorrelationId(string correlationId);
        IMessageBuilder ReplyTo(string replyTo);
        IMessageBuilder Expiration(string expiration);
        IMessageBuilder MessageId(string messageId);
        IMessageBuilder Timestamp(DateTimeOffset? timestamp);
        IMessageBuilder Type(string type);
        IMessageBuilder UserId(string userId);
        IMessageBuilder ApplicationId(string applicationId);
    }
}