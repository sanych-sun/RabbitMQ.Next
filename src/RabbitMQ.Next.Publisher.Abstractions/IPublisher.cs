using System;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Publisher.Abstractions
{
    public interface IPublisher : IAsyncDisposable
    {
        ValueTask PublishAsync<TContent>(TContent content, Action<IMessageBuilder> propertiesBuilder = null, PublishFlags flags = PublishFlags.None, CancellationToken cancellationToken = default);

        ValueTask PublishAsync<TState, TContent>(TState state, TContent content, Action<TState, IMessageBuilder> propertiesBuilder, PublishFlags flags = PublishFlags.None, CancellationToken cancellationToken = default);
    }
}