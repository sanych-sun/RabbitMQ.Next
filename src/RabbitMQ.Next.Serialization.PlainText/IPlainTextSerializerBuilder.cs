namespace RabbitMQ.Next.Serialization.PlainText;

public interface IPlainTextSerializerBuilder
{
    IPlainTextSerializerBuilder UseConverter<T>(IConverter<T> converter);
    
    IPlainTextSerializerBuilder UseConverter<T>(IConverter<T> converter, bool useNullableWrapper)
        where T: struct;
}