namespace RabbitMQ.Next.TopologyBuilder;

public interface IExchangeBindingBuilder
{
    string Source { get; }

    string Destination { get; }

    IExchangeBindingBuilder RoutingKey(string routingKey);

    IExchangeBindingBuilder Argument(string key, object value);
}