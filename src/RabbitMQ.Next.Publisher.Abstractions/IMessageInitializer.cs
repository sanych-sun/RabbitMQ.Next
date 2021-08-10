namespace RabbitMQ.Next.Publisher.Abstractions
{
    public interface IMessageInitializer
    {
        void Apply<TPayload>(TPayload payload, IMessageBuilder message);
    }
}