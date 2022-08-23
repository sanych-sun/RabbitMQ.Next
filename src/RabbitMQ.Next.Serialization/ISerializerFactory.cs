using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Serialization;

public interface ISerializerFactory
{
    ISerializer Get(IMessageProperties message);
}