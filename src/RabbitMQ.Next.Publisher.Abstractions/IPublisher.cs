using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Publisher.Abstractions
{
    public interface IPublisher : IAsyncDisposable
    {
        ValueTask PublishAsync<TContent>(TContent content, string routingKey = null, IMessageProperties properties = null, PublishFlags flags = PublishFlags.None, CancellationToken cancellationToken = default);

        ValueTask CompleteAsync();
    }
}