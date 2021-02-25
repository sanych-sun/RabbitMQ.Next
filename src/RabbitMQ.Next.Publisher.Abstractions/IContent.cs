namespace RabbitMQ.Next.Publisher.Abstractions
{
    public interface IContent
    {
        T GetContent<T>();
    }
}