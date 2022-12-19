namespace RabbitMQ.Next.TopologyBuilder;

public interface IStreamQueueDeclaration
{
    IStreamQueueDeclaration Argument(string key, object value);
}