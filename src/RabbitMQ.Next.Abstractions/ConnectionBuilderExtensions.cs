using RabbitMQ.Next.Abstractions.Auth;

namespace RabbitMQ.Next.Abstractions
{
    public static class ConnectionBuilderExtensions
    {
        public static IConnectionBuilder AuthPlain(this IConnectionBuilder builder, string user, string password)
        {
            builder.Auth(new PlainAuthMechanism(user, password));
            return builder;
        }
    }
}