using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.MessagePublisher.Abstractions
{
    public interface IPublisher<in TContent>
    {
        Task PublishAsync(string exchange, TContent content, string routingKey, MessageProperties properties = default);
    }
}