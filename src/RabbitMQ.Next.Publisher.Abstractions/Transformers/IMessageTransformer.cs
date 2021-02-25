namespace RabbitMQ.Next.Publisher.Abstractions.Transformers
{
    public interface IMessageTransformer
    {
        void Apply<TPayload>(TPayload payload, IMessageBuilder message);
    }
}