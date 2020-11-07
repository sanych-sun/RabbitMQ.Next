using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.MessagePublisher.Abstractions
{
    public interface IMessagePublisher<in T>
    {
        bool IsReady { get; }

        ValueTask WaitForReadyAsync();

        Task PublishAsync<TMessage>(string exchange, TMessage message, MessageProperties properties = default)
            where TMessage : T;
    }
}