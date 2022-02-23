using System;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Publisher
{
    public interface IReturnedMessageHandler : IDisposable
    {
        ValueTask<bool> TryHandleAsync(ReturnedMessage message, IContentAccessor content);
    }
}