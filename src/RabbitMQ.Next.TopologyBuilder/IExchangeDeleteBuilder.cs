namespace RabbitMQ.Next.TopologyBuilder;

public interface IExchangeDeleteBuilder
{
    string Name { get; }

    IExchangeDeleteBuilder CancelBindings();
}