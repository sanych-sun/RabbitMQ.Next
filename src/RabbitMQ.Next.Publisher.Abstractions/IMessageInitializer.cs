namespace RabbitMQ.Next.Publisher;

public interface IMessageInitializer
{
    void Apply<TPayload>(TPayload payload, IMessageBuilder message);
}