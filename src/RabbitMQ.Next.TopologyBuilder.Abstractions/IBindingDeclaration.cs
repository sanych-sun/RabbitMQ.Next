namespace RabbitMQ.Next.TopologyBuilder;

public interface IBindingDeclaration
{
    IBindingDeclaration Argument(string key, object value);
}