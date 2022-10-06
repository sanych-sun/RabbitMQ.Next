using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next;

public interface IConnectionBuilder
{
    IConnectionBuilder Auth(IAuthMechanism mechanism);

    IConnectionBuilder VirtualHost(string vhost);

    IConnectionBuilder Endpoint(string host, int port, bool ssl = false);

    IConnectionBuilder ClientProperty(string key, object value);

    IConnectionBuilder Locale(string locale);

    IConnectionBuilder MaxFrameSize(int sizeBytes);

    Task<IConnection> ConnectAsync(CancellationToken cancellation = default);
}