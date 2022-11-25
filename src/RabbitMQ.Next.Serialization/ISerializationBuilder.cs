namespace RabbitMQ.Next.Serialization;

public interface ISerializationBuilder<out TBuilder>
{
    TBuilder UseSerializer(ISerializer serializer);
}