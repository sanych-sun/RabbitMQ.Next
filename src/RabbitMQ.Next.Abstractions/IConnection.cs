using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Channels;

namespace RabbitMQ.Next.Abstractions
{
    public interface IConnection
    {
        Task<IChannel> CreateChannelAsync();

        Task CloseAsync();

        ConnectionState State { get; }

        event EventHandler<ConnectionStateEventArgs> StateChanged;
    }
}