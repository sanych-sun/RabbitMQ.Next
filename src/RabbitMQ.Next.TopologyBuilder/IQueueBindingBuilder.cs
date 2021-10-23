namespace RabbitMQ.Next.TopologyBuilder
{
    public interface IQueueBindingBuilder
    {
        string Exchange { get; }

        string Queue { get; }

        IQueueBindingBuilder RoutingKey(string routingKey);

        IQueueBindingBuilder Argument(string key, object value);
    }
}