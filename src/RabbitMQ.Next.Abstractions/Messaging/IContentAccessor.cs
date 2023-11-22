namespace RabbitMQ.Next.Messaging;

public interface IContentAccessor
{
    public T Get<T>();
}