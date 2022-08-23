using System;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Publisher;

internal class ReturnedMessageDelegateHandler : IReturnedMessageHandler
{
    private Func<IReturnedMessage, Task<bool>> wrapped;

    public ReturnedMessageDelegateHandler(Func<IReturnedMessage, Task<bool>> handler)
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

    public Task<bool> TryHandleAsync(IReturnedMessage message)
    {
        if (this.wrapped == null)
        {
            throw new ObjectDisposedException(nameof(ReturnedMessageDelegateHandler));
        }

        return this.wrapped(message);
    }
}