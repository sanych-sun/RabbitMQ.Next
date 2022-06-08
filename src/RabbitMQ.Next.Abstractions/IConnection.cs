using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Methods;
using RabbitMQ.Next.Serialization;

namespace RabbitMQ.Next
{
    public interface IConnection : IAsyncDisposable
    {
        Task<IChannel> OpenChannelAsync(CancellationToken cancellationToken = default);

        ConnectionState State { get; }

        IMethodRegistry MethodRegistry { get; }
        
        ISerializerFactory SerializerFactory { get; }
    }
}