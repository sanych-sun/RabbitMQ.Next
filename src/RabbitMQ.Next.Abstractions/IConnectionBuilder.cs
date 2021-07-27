using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Abstractions
{
    public interface IConnectionBuilder
    {
        IConnectionBuilder Auth(IAuthMechanism mechanism);

        IConnectionBuilder VirtualHost(string vhost);

        IConnectionBuilder AddEndpoint(string host, int port, bool ssl = false);

        IConnectionBuilder ConfigureMethodRegistry(Action<IMethodRegistryBuilder> builder);

        IConnectionBuilder ClientProperty(string key, object value);

        IConnectionBuilder Locale(string locale);

        IConnectionBuilder FrameSize(int sizeBytes);

        Task<IConnection> ConnectAsync();
    }
}