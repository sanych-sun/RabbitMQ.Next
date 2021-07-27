using System.Collections.Generic;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Transport;

namespace RabbitMQ.Next
{
    internal sealed class ConnectionFactory : IConnectionFactory
    {
        public static readonly IConnectionFactory Default = new ConnectionFactory();

        public Task<IConnection> ConnectAsync(
            IReadOnlyList<Endpoint> endpoints,
            string virtualHost,
            IAuthMechanism authMechanism,
            string locale,
            IReadOnlyDictionary<string, object> clientProperties,
            IMethodRegistry methodRegistry,
            int frameSize)
            => Connection.ConnectAsync(endpoints, virtualHost, authMechanism, locale, clientProperties, methodRegistry, frameSize);
    }
}