namespace RabbitMQ.Next.Consumer
{
    public interface IQueueConsumerBuilder
    {
        string Queue { get; }

        IQueueConsumerBuilder NoLocal();

        IQueueConsumerBuilder Exclusive();

        IQueueConsumerBuilder Argument(string key, object value);

        IQueueConsumerBuilder ConsumerTag(string consumerTag);
    }
}