namespace RabbitMQ.Next.TopologyBuilder;

public interface IQueueDeleteBuilder
{
    string Name { get; }

    IQueueDeleteBuilder CancelConsumers();

    IQueueDeleteBuilder DiscardMessages();
}