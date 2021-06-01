using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Buffers;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Events;

namespace RabbitMQ.Next.Abstractions
{
    public interface IConnection : IAsyncDisposable
    {
        IEventSource<ConnectionState> StateChanged { get; }

        Task<IChannel> CreateChannelAsync(IReadOnlyList<IMethodHandler> handlers = null, CancellationToken cancellationToken = default);

        Task ConnectAsync();

        Task CloseAsync();

        ConnectionState State { get; }

        IBufferPool BufferPool { get; }

        IConnectionDetails Details { get; }
    }
}