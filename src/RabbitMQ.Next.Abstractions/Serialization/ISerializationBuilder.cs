namespace RabbitMQ.Next.Serialization
{
    public interface ISerializationBuilder
    {
        ISerializationBuilder DefaultSerializer(ISerializer serializer);
        
        ISerializationBuilder UseSerializer(ISerializer serializer, string contentType);
    }
}