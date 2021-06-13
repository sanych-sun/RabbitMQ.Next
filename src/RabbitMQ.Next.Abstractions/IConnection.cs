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
        Task<IChannel> OpenChannelAsync(IReadOnlyList<IMethodHandler> handlers = null, CancellationToken cancellationToken = default);

        ConnectionState State { get; }

        IBufferPool BufferPool { get; }

        IConnectionDetails Details { get; }
    }
}