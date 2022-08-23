using RabbitMQ.Next.Messaging;

namespace RabbitMQ.Next.Serialization;

public interface ISerializationBuilder<out TBuilder>
{
    TBuilder DefaultSerializer(ISerializer serializer);
        
    TBuilder UseSerializer(ISerializer serializer, Func<IMessageProperties, bool> predicate);
}