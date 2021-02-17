using System;
using System.Threading.Tasks;

namespace RabbitMQ.Next.MessagePublisher.Abstractions
{
    public interface IPublisherChannel : IPublisher, IAsyncDisposable
    {
        ValueTask CompleteAsync();
    }
}