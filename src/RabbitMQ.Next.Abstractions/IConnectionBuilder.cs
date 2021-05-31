using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Abstractions
{
    public interface IConnectionBuilder
    {
        IConnectionBuilder Auth(IAuthMechanism mechanism);

        IConnectionBuilder VirtualHost(string vhost);

        IConnectionBuilder AddEndpoint(Endpoint endpoint);

        IConnectionBuilder ConfigureMethodRegistry(Action<IMethodRegistryBuilder> builder);

        IConnectionBuilder ClientProperty(string key, object value);

        IConnectionBuilder Locale(string locale);

        IConnection Build();
    }
}