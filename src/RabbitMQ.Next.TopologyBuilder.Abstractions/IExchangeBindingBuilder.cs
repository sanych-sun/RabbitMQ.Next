using System.Collections.Generic;

namespace RabbitMQ.Next.TopologyBuilder.Abstractions
{
    public interface IExchangeBindingBuilder
    {
        string Source { get; }

        string Destination { get; }

        string RoutingKey { get; set; }

        void SetArgument(string key, object value);
    }
}