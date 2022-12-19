namespace RabbitMQ.Next.TopologyBuilder;

public interface IExchangeDeclaration
{
    IExchangeDeclaration Internal();

    IExchangeDeclaration AutoDelete();

    IExchangeDeclaration Argument(string key, object value);
}