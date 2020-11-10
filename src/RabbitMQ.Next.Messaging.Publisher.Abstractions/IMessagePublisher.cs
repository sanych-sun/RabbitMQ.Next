using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.MessagePublisher.Abstractions
{
    public interface IMessagePublisher<in TLimit>
    {
        Task PublishAsync<TMessage>(string exchange, TMessage message, string routingKey, MessageProperties properties = default)
            where TMessage : TLimit;
    }
}