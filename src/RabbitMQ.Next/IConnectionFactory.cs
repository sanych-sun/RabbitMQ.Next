using System.Collections.Generic;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Methods;
using RabbitMQ.Next.Transport;

namespace RabbitMQ.Next
{
    internal interface IConnectionFactory
    {
        Task<IConnection> ConnectAsync(
            IReadOnlyList<Endpoint> endpoints,
            string virtualHost,
            IAuthMechanism authMechanism,
            string locale,
            IReadOnlyDictionary<string, object> clientProperties,
            IMethodRegistry methodRegistry,
            int frameSize);
    }
}