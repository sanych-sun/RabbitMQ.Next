namespace RabbitMQ.Next.TopologyBuilder;

public interface IClassicQueueDeclaration
{
    IClassicQueueDeclaration AutoDelete();
    
    IClassicQueueDeclaration Exclusive();
    
    IClassicQueueDeclaration Argument(string key, object value);
}