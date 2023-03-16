using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Publisher;

public interface IReturnMiddleware
{
    ValueTask InvokeAsync(IReturnedMessage message, CancellationToken cancellation);
}