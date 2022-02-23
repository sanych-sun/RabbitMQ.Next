using System;

namespace RabbitMQ.Next.Publisher.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
    public class RoutingKeyAttribute : MessageAttributeBase
    {
        public RoutingKeyAttribute(string routingKey)
        {
            if (string.IsNullOrWhiteSpace(routingKey))
            {
                throw new ArgumentNullException(nameof(routingKey));
            }

            this.RoutingKey = routingKey;
        }
        
        public string RoutingKey { get; }

        public override void Apply(IMessageBuilder message)
            => message.RoutingKey(this.RoutingKey);
    }
}