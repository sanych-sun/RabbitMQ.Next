using System;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.MessagePublisher.Abstractions
{
    public interface IPublisher : IAsyncDisposable
    {
        ValueTask PublishAsync<TContent>(TContent content, MessageHeader header = null, CancellationToken cancellation = default);

        ValueTask CompleteAsync();
    }
}