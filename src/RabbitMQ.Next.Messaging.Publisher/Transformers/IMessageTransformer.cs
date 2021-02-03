using RabbitMQ.Next.MessagePublisher.Abstractions;

namespace RabbitMQ.Next.MessagePublisher.Transformers
{
    public interface IMessageTransformer
    {
        void Apply<TPayload>(TPayload payload, MessageHeader header);
    }
}