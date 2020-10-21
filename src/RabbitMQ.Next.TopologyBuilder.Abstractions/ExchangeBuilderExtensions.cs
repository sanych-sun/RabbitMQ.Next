namespace RabbitMQ.Next.TopologyBuilder.Abstractions
{
    public static class ExchangeBuilderExtensions
    {
        public static IExchangeBuilder WithAlternateExchange(this IExchangeBuilder builder, string alternateExchange)
        {
            builder.SetArgument("alternate-exchange", alternateExchange);
            return builder;
        }
    }
}