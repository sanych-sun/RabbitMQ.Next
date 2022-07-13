namespace RabbitMQ.Next.Serialization.PlainText;

public interface IPlainTextSerializerBuilder
{
    IPlainTextSerializerBuilder AsDefault();

    IPlainTextSerializerBuilder ContentType(string contentType);

    IPlainTextSerializerBuilder UseConverter(IConverter converter);
}