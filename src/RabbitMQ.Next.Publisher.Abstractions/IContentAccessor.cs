namespace RabbitMQ.Next.Publisher.Abstractions
{
    public interface IContentAccessor
    {
        T GetContent<T>();
    }
}