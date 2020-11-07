namespace RabbitMQ.Next.Abstractions.Channels
{
    public interface IBufferPool
    {
        IBufferWriter Create();
    }
}