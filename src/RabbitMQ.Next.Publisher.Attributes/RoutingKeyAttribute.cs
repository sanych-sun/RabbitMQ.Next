using System;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;

namespace RabbitMQ.Next.Publisher.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false)]
    public class RoutingKeyAttribute : MessageAttributeBase
    {
        public RoutingKeyAttribute(string routingKey)
        {
            this.RoutingKey = routingKey;
        }
        
        public string RoutingKey { get; }

        public override void Apply(IMessageBuilder message)
        {
            if (string.IsNullOrEmpty(message.RoutingKey))
            {
                message.SetRoutingKey(this.RoutingKey);
            }
        }
    }
}