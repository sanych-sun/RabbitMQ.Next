namespace RabbitMQ.Next.Serialization.MessagePack
{
    public interface IMessagePackSerializerBuilder
    {
        IMessagePackSerializerBuilder AsDefault();

        IMessagePackSerializerBuilder ContentType(string contentType);
    }
}