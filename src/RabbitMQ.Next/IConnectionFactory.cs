using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next;

internal interface IConnectionFactory
{
    Task<IConnection> ConnectAsync(ConnectionSettings settings, CancellationToken cancellation = default);
}