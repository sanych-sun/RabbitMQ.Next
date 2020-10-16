using System.Collections.Generic;

namespace RabbitMQ.Next.TopologyBuilder.Abstractions
{
    public interface IExchangeBuilder
    {
        string Name { get; }

        string Type { get; }

        ExchangeFlags Flags { get; set; }

        IDictionary<string, object> Arguments { get; }
    }
}