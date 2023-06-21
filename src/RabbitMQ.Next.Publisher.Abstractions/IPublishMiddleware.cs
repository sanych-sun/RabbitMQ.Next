using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;

namespace RabbitMQ.Next.Publisher;

public interface IPublishMiddleware
{
    ValueTask InitAsync(IChannel channel, CancellationToken cancellation);
    
    ValueTask<ulong> InvokeAsync<TContent>(TContent content, IMessageBuilder message, CancellationToken cancellation);
}