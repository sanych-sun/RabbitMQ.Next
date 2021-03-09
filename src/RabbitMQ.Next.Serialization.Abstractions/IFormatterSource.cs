namespace RabbitMQ.Next.Serialization.Abstractions
{
    public interface IFormatterSource
    {
        bool TryGetFormatter<TContent>(out ITypeFormatter typeFormatter);
    }
}