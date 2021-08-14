namespace RabbitMQ.Next.Consumer.Abstractions
{
    public interface IContentAccessor
    {
        T GetContent<T>();
    }
}