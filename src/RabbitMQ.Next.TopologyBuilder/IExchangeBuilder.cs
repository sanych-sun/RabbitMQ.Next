namespace RabbitMQ.Next.TopologyBuilder
{
    public interface IExchangeBuilder
    {
        string Name { get; }

        string Type { get; }

        IExchangeBuilder Transient();

        IExchangeBuilder Internal();

        IExchangeBuilder AutoDelete();

        IExchangeBuilder Argument(string key, object value);
    }
}