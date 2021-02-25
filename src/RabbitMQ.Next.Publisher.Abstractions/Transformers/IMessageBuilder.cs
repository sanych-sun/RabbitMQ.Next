using System;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Publisher.Abstractions.Transformers
{
    public interface IMessageBuilder : IMessageProperties
    {
        string Exchange { get; set; }

        string RoutingKey { get; set; }

        public PublishFlags PublishFlags { get; set; }

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