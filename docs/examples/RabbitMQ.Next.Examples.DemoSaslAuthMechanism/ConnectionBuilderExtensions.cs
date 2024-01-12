namespace RabbitMQ.Next.Examples.DemoSaslAuthMechanism;

public static class ConnectionBuilderExtensions
{
    public static IConnectionBuilder WithRabbitCrDemoAuth(this IConnectionBuilder builder, string userName, string password)
    {
        builder.Auth(new RabbitCrDemoAuthMechanism(userName, password));
        return builder;
    }
}