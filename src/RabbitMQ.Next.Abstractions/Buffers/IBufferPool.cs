namespace RabbitMQ.Next.Abstractions.Buffers
{
    public interface IBufferPool
    {
        IBufferWriter Create();

        MemoryBlock CreateMemory(int size = 0);
    }
}