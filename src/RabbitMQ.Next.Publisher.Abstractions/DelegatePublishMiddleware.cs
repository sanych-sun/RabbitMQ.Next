using System;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Publisher;

internal sealed class DelegatePublishMiddleware: IPublishMiddleware
{
    private readonly IPublishMiddleware next;
    private readonly Action<object, IMessageBuilder> middleware;

    public DelegatePublishMiddleware(IPublishMiddleware next, Action<object, IMessageBuilder> middleware)
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

    public ValueTask InvokeAsync<TContent>(TContent content, IMessageBuilder message, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        
        this.middleware.Invoke(content, message);
        return this.next.InvokeAsync(content, message, cancellation);
    }
}