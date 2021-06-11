using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Buffers;
using RabbitMQ.Next.Abstractions.Channels;

namespace RabbitMQ.Next.Abstractions
{
    public interface IConnection : IAsyncDisposable
    {
        Task<IChannel> CreateChannelAsync(IReadOnlyList<IMethodHandler> handlers = null, CancellationToken cancellationToken = default);

        Task ConnectAsync();

        Task CloseAsync();

        ConnectionState State { get; }

        IBufferPool BufferPool { get; }

        IConnectionDetails Details { get; }
    }
}