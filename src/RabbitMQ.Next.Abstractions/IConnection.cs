using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next
{
    public interface IConnection : IAsyncDisposable
    {
        Task<IChannel> OpenChannelAsync(IReadOnlyList<IFrameHandler> handlers = null, CancellationToken cancellationToken = default);

        ConnectionState State { get; }

        IMethodRegistry MethodRegistry { get; }
    }
}