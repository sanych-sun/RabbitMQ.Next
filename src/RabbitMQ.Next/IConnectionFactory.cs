using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Methods;
using RabbitMQ.Next.Serialization;

namespace RabbitMQ.Next;

internal interface IConnectionFactory
{
    Task<IConnection> ConnectAsync(ConnectionSettings settings, IMethodRegistry registry, CancellationToken cancellation);
}