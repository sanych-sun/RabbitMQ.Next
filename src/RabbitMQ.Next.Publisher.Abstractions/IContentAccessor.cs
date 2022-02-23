using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Publisher
{
    public interface IContentAccessor
    {
        IMessageProperties Properties { get; }

        T GetContent<T>();
    }
}