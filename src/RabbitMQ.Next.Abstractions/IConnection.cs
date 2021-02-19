using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Buffers;
using RabbitMQ.Next.Abstractions.Channels;

namespace RabbitMQ.Next.Abstractions
{
    public interface IConnection
    {
        IEventSource<ConnectionStateChanged> StateChanged { get; }

        Task<IChannel> CreateChannelAsync(CancellationToken cancellationToken = default);

        Task CloseAsync();

        ConnectionState State { get; }

        IBufferPool BufferPool { get; }
    }
}