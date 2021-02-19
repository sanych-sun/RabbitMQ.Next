namespace RabbitMQ.Next.Abstractions.Buffers
{
    public interface IBufferPool
    {
        IBufferWriter Create();
    }
}