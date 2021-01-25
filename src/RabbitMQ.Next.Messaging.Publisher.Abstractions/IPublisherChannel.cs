using System.Threading.Tasks;

namespace RabbitMQ.Next.MessagePublisher.Abstractions
{
    public interface IPublisherChannel<in TContent> : IPublisher<TContent>
    {
        ValueTask CompleteAsync();
    }
}