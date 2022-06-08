namespace RabbitMQ.Next.Serialization
{
    public interface ISerializerFactory
    {
        ISerializer Get(string contentType);
    }
}