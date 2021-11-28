using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next
{
    internal interface IConnectionFactory
    {
        Task<IConnection> ConnectAsync(ConnectionSettings settings, IMethodRegistry registry, CancellationToken cancellation);
    }
}