using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.MessagePublisher.Abstractions
{
    public interface IPublisher
    {
        ValueTask PublishAsync<TContent>(TContent content, MessageHeader header = null, CancellationToken cancellation = default);
    }
}