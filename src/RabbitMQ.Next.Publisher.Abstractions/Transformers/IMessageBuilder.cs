using System;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Publisher.Abstractions.Transformers
{
    public interface IMessageBuilder : IMessageProperties
    {
        string Exchange { get; }

        string RoutingKey { get; }

        public PublishFlags PublishFlags { get; }

        void SetExchange(string value);

        void SetRoutingKey(string value);

        void SetPublishFlags(PublishFlags value);

        void SetHeader (string key, object value);

        void SetContentType (string value);

        void SetContentEncoding (string value);

        void SetDeliveryMode (DeliveryMode value);

        void SetPriority (byte value);

        void SetCorrelationId(string value);

        void SetReplyTo(string value);

        void SetExpiration(string value);

        void SetMessageId(string value);

        void SetTimestamp(DateTimeOffset value);

        void SetType (string value);

        void SetUserId (string value);

        void SetApplicationId (string value);
    }
}