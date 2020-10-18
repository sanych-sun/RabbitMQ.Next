using System.Collections.Generic;

namespace RabbitMQ.Next.TopologyBuilder.Abstractions
{
    public interface IQueueBindingBuilder
    {
        string Exchange { get; }

        string Queue { get; }

        string RoutingKey { get; set; }

        void SetArgument(string key, object value);
    }
}