namespace RabbitMQ.Next.Serialization.Abstractions
{
    public interface IFormatterSource
    {
        IFormatter GetFormatter<TContent>();
    }
}