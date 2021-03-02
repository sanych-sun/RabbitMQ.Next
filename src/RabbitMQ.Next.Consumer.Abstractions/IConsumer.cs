using System;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Consumer.Abstractions
{
    public interface IConsumer : IAsyncDisposable
    {
        Task ConsumeAsync(CancellationToken cancellation = default);
    }
}