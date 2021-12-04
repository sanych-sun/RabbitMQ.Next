using RabbitMQ.Next.Abstractions.Messaging;

namespace RabbitMQ.Next.Serialization.Abstractions
{
    public interface ISerializerFactory
    {
        ISerializer Get(IMessageProperties message);
    }
}