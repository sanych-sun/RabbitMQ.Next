using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Channels;

namespace RabbitMQ.Next.Abstractions
{
    public interface IConnection
    {
        IEventSource<ConnectionStateChanged> StateChanged { get; }

        Task<IChannel> CreateChannelAsync();

        Task CloseAsync();

        ConnectionState State { get; }

        IBufferPool BufferPool { get; }
    }
}