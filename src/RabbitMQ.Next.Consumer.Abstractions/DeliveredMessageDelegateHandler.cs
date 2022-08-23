using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Consumer;

internal class DeliveredMessageDelegateHandler : IDeliveredMessageHandler
{
    private Func<IDeliveredMessage, Task<bool>> wrapped;

    public DeliveredMessageDelegateHandler(Func<IDeliveredMessage, Task<bool>> handler)
    {
        if (handler == null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        this.wrapped = handler;
    }

    public void Dispose()
    {
        this.wrapped = null;
    }

    public Task<bool> TryHandleAsync(IDeliveredMessage message)
    {
        if (this.wrapped == null)
        {
            throw new ObjectDisposedException(nameof(DeliveredMessageDelegateHandler));
        }

        return this.wrapped(message);
    }
}