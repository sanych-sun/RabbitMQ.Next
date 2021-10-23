namespace RabbitMQ.Next.TopologyBuilder
{
    public interface IQueueBuilder
    {
        string Name { get; }

        IQueueBuilder Flags(QueueFlags flag);

        IQueueBuilder Argument(string key, object value);
    }
}