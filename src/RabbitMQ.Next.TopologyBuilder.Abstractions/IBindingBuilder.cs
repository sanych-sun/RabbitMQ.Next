using System.Collections.Generic;

namespace RabbitMQ.Next.TopologyBuilder.Abstractions
{
    public interface IBindingBuilder
    {
        BindingTarget Type { get; }

        string Source { get; }

        string Destination { get; }

        string RoutingKey { get; set; }

        IDictionary<string, object> Arguments { get; }
    }
}