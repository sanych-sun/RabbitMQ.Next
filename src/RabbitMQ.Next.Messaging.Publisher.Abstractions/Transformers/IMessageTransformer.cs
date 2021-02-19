namespace RabbitMQ.Next.MessagePublisher.Abstractions.Transformers
{
    public interface IMessageTransformer
    {
        void Apply<TPayload>(TPayload payload, IMessageBuilder message);
    }
}