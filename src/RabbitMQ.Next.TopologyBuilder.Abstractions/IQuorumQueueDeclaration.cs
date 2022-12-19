namespace RabbitMQ.Next.TopologyBuilder;

public interface IQuorumQueueDeclaration
{
    IQuorumQueueDeclaration AutoDelete();

    IQuorumQueueDeclaration Argument(string key, object value);
}