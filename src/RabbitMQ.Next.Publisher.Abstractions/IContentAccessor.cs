using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Publisher.Abstractions
{
    public interface IContentAccessor
    {
        IMessageProperties Properties { get; }

        T GetContent<T>();
    }
}