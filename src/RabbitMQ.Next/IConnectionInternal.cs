using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Next.Buffers;
using RabbitMQ.Next.Channels;

namespace RabbitMQ.Next
{
    internal interface IConnectionInternal : IConnection
    {
        ValueTask WriteToSocketAsync(MemoryBlock memory, CancellationToken cancellation = default);

        ObjectPool<MemoryBlock> MemoryPool { get; }

        ObjectPool<FrameBuilder> FrameBuilderPool { get; }
    }
}