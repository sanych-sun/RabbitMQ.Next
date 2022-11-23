using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Next.Buffers;

namespace RabbitMQ.Next;

internal interface IConnectionInternal : IConnection
{
    Task WriteToSocketAsync(MemoryBlock memory, CancellationToken cancellation = default);

    ObjectPool<MemoryBlock> MemoryPool { get; }
}