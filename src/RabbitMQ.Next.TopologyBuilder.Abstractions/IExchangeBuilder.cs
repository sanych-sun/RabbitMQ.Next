namespace RabbitMQ.Next.TopologyBuilder.Abstractions
{
    public interface IExchangeBuilder
    {
        string Name { get; }

        string Type { get; }

        ExchangeFlags Flags { get; set; }

        void SetArgument(string key, object value);
    }
}