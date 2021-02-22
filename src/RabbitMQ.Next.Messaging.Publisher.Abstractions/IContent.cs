namespace RabbitMQ.Next.MessagePublisher.Abstractions
{
    public interface IContent
    {
        T GetContent<T>();
    }
}