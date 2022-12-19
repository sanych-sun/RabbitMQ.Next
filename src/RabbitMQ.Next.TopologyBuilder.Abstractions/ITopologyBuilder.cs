namespace RabbitMQ.Next.TopologyBuilder;

public interface ITopologyBuilder
{
    IQueueBuilder Queue { get; }
    
    IExchangeBuilder Exchange { get; }
}