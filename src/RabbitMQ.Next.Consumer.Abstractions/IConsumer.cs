using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Consumer;

public interface IConsumer : IAsyncDisposable
{
    Task ConsumeAsync(Func<IDeliveredMessage, IContentAccessor, Task> handler, CancellationToken cancellation = default);
}