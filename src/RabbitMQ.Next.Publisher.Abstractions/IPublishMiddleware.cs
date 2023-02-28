using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Publisher;

public interface IPublishMiddleware
{
    ValueTask InvokeAsync<TContent>(TContent content, IMessageBuilder message, CancellationToken cancellation);
}