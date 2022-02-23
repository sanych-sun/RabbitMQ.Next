using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Consumer
{
    public interface IContentAccessor
    {
        IMessageProperties Properties { get; }

        T GetContent<T>();
    }
}