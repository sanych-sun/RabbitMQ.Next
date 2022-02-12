namespace RabbitMQ.Next.TopologyBuilder
{
    public interface IQueueBuilder
    {
        string Name { get; }

        IQueueBuilder Transient();

        IQueueBuilder Exclusive();

        IQueueBuilder AutoDelete();

        IQueueBuilder Argument(string key, object value);
    }
}