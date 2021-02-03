using System;

namespace RabbitMQ.Next.MessagePublisher.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false)]
    public class RoutingKeyAttribute : Attribute
    {
        public RoutingKeyAttribute(string routingKey)
        {
            this.RoutingKey = routingKey;
        }
        
        public string RoutingKey { get; }
    }
}