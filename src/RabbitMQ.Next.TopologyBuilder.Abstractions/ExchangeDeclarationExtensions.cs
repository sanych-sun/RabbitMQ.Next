namespace RabbitMQ.Next.TopologyBuilder;

public static class ExchangeDeclarationExtensions
{
    public static IExchangeDeclaration WithAlternateExchange(this IExchangeDeclaration builder, string alternateExchange)
    {
        builder.Argument("alternate-exchange", alternateExchange);
        return builder;
    }
}