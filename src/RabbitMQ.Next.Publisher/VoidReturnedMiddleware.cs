using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Publisher;

internal sealed class VoidReturnedMiddleware: IReturnMiddleware
{
    public ValueTask InvokeAsync(IReturnedMessage message, CancellationToken cancellation) 
        => default;
}