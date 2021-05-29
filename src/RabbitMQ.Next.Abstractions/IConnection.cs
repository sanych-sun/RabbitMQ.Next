using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Buffers;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Abstractions
{
    public interface IConnection : IAsyncDisposable
    {
        IEventSource<ConnectionState> StateChanged { get; }

        Task<IChannel> CreateChannelAsync(IEnumerable<IMethodHandler> handlers = null, CancellationToken cancellationToken = default);

        Task CloseAsync();

        ConnectionState State { get; }

        IMethodRegistry MethodRegistry { get; }

        IBufferPool BufferPool { get; }

        IConnectionDetails Details { get; }
    }
}