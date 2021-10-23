namespace RabbitMQ.Next.TopologyBuilder
{
    public static class ExchangeBuilderExtensions
    {
        public static IExchangeBuilder WithAlternateExchange(this IExchangeBuilder builder, string alternateExchange)
        {
            builder.Argument("alternate-exchange", alternateExchange);
            return builder;
        }
    }
}