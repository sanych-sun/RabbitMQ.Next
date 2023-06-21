using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Consumer;

public interface IConsumerMiddleware
{
    ValueTask InvokeAsync(IDeliveredMessage message, CancellationToken cancellation);
}