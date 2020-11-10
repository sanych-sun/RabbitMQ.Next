namespace RabbitMQ.Next.Abstractions
{
    public interface IBufferPool
    {
        IBufferWriter Create();
    }
}