namespace RabbitMQ.Next.Serialization.PlainText
{
    public interface IPlainTextSerializerBuilder
    {
        IPlainTextSerializerBuilder AsDefault();

        IPlainTextSerializerBuilder ContentType(string contentType);

        IPlainTextSerializerBuilder UseFormatter(IFormatter formatter);
    }
}