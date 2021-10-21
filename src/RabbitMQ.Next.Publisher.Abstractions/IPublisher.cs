using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Channels;

namespace RabbitMQ.Next.Publisher.Abstractions
{
    public interface IPublisher : IAsyncDisposable
    {
        ValueTask PublishAsync<TState, TContent>(TState state, TContent content, Action<TState, IMessageBuilder> propertiesBuilder, PublishFlags flags = PublishFlags.None, CancellationToken cancellation = default);
    }
}