namespace RabbitMQ.Next.Buffers
{
    internal interface IMemoryPool
    {
        MemoryBlock Get();
    }
}