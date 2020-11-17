using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.MessagePublisher.Abstractions
{
    public interface IPublisherChannel<in TContent>
    {
        ValueTask WriteAsync(TContent content, string routingKey, MessageProperties properties = default, bool mandatory = false, bool immediate = false, CancellationToken cancellation = default);

        ValueTask CompleteAsync();
    }
}