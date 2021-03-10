namespace RabbitMQ.Next.Abstractions.Buffers
{
    public interface IBufferPool
    {
        IBufferWriter Create();

        MemoryOwner CreateMemory(int minSize = 0);
    }
}