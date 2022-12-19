namespace RabbitMQ.Next.TopologyBuilder;

public interface IQueueDeletion
{
    IQueueDeletion CancelConsumers();

    IQueueDeletion DiscardMessages();
}