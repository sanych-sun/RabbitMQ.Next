using RabbitMQ.Next.Abstractions.Buffers;

namespace RabbitMQ.Next.Transport.Buffers
{
    internal interface IBufferPoolInternal : IBufferPool
    {
        void SetBufferSize(int maxSize);
    }
}