namespace RabbitMQ.Next.Buffers
{
    internal interface IBufferPool
    {
        MemoryBlock CreateMemory();
    }
}