using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.MessagePublisher.Abstractions
{
    public interface IPublisher<in TContent>
    {
        ValueTask PublishAsync(TContent content, MessageHeader header = null, CancellationToken cancellation = default);
    }
}