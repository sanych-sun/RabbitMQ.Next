using System;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Publisher;

internal sealed class DelegateReturnMiddleware: IReturnMiddleware
{
    private readonly IReturnMiddleware next;
    private readonly Action<IReturnedMessage> middleware;

    public DelegateReturnMiddleware(IReturnMiddleware next, Action<IReturnedMessage> middleware)
    {
        if (middleware == null)
        {
            throw new ArgumentNullException(nameof(middleware));
        }

        if (next == null)
        {
            throw new ArgumentNullException(nameof(next));
        }
        
        this.next = next;
        this.middleware = middleware;
    }

    public ValueTask InvokeAsync(IReturnedMessage message, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        
        this.middleware.Invoke(message);
        return this.next.InvokeAsync(message, cancellation);
    }
}