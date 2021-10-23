namespace RabbitMQ.Next.TopologyBuilder
{
    public interface IExchangeBuilder
    {
        string Name { get; }

        string Type { get; }

        IExchangeBuilder Flags(ExchangeFlags flag);

        IExchangeBuilder Argument(string key, object value);
    }
}