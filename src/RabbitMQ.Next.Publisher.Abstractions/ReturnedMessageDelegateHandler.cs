using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Publisher;

internal class ReturnedMessageDelegateHandler : IReturnedMessageHandler
{
    private Func<ReturnedMessage, IContent, Task<bool>> wrapped;

    public ReturnedMessageDelegateHandler(Func<ReturnedMessage, IContent, Task<bool>> handler)
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

    public Task<bool> TryHandleAsync(ReturnedMessage message, IContent content)
    {
        if (this.wrapped == null)
        {
            throw new ObjectDisposedException(nameof(ReturnedMessageDelegateHandler));
        }

        return this.wrapped(message, content);
    }
}