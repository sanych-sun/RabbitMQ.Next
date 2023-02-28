using System;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Publisher;

public interface IPublisher : IAsyncDisposable
{
    Task PublishAsync<TContent>(TContent content, Action<IMessageBuilder> propertiesBuilder = null, CancellationToken cancellation = default);

    Task PublishAsync<TState, TContent>(TState state, TContent content, Action<TState, IMessageBuilder> propertiesBuilder = null, CancellationToken cancellation = default);
}