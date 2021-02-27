namespace RabbitMQ.Next.Consumer.Abstractions
{
    public interface IContent
    {
        T GetContent<T>();
    }
}