using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;

namespace RabbitMQ.Next.Publisher
{
    public interface IPublisher : IAsyncDisposable
    {
        ValueTask PublishAsync<TContent>(TContent content, Action<IMessageBuilder> propertiesBuilder = null, PublishFlags flags = PublishFlags.None, CancellationToken cancellation = default);

        ValueTask PublishAsync<TState, TContent>(TState state, TContent content, Action<TState, IMessageBuilder> propertiesBuilder = null, PublishFlags flags = PublishFlags.None, CancellationToken cancellation = default);
    }
}