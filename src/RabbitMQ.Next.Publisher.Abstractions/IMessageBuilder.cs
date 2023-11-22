using System;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Publisher;

public interface IMessageBuilder: IMessageProperties
{
    string Exchange { get; }
    
    string RoutingKey { get; }
    
    bool Mandatory { get; }
    
    bool Immediate { get; }
    
    Type ClrType { get; }
    
    IMessageBuilder SetMandatory();
    
    IMessageBuilder SetImmediate();
    
    IMessageBuilder SetRoutingKey(string routingKey);

    IMessageBuilder SetContentType(string contentType);

    IMessageBuilder SetContentEncoding(string contentEncoding);

    IMessageBuilder SetHeader(string key, object value);

    IMessageBuilder SetDeliveryMode(DeliveryMode deliveryMode);

    IMessageBuilder SetPriority(byte priority);

    IMessageBuilder SetCorrelationId(string correlationId);

    IMessageBuilder SetReplyTo(string replyTo);

    IMessageBuilder SetExpiration(string expiration);

    IMessageBuilder SetMessageId(string messageId);

    IMessageBuilder SetTimestamp(DateTimeOffset timestamp);

    IMessageBuilder SetType(string type);

    IMessageBuilder SetUserId(string userId);

    IMessageBuilder SetApplicationId(string applicationId);
}