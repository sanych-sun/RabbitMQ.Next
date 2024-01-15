using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Serialization;

namespace RabbitMQ.Next;

public interface IConnectionBuilder: ISerializationBuilder<IConnectionBuilder>
{
    IConnectionBuilder Auth(IAuthMechanism mechanism);

    IConnectionBuilder VirtualHost(string vhost);

    IConnectionBuilder Endpoint(string host, int port, bool ssl = false);

    IConnectionBuilder ClientProperty(string key, object value);

    IConnectionBuilder Locale(string locale);

    IConnectionBuilder MaxFrameSize(int sizeBytes);
    
    IConnection Build();
}