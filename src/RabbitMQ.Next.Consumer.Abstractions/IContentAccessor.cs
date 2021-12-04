using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Consumer.Abstractions
{
    public interface IContentAccessor
    {
        IMessageProperties Properties { get; }

        T GetContent<T>();
    }
}