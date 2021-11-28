using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Abstractions
{
    public interface IConnection : IAsyncDisposable
    {
        Task<IChannel> OpenChannelAsync(IReadOnlyList<IFrameHandler> handlers = null, CancellationToken cancellationToken = default);

        ConnectionState State { get; }

        IMethodRegistry MethodRegistry { get; }
    }
}