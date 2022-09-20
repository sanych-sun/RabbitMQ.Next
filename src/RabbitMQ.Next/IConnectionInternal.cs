using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Next.Buffers;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Messaging;
using RabbitMQ.Next.Transport.Methods.Registry;

namespace RabbitMQ.Next;

internal interface IConnectionInternal : IConnection
{
    IMethodRegistry MethodRegistry { get; }
    
    ValueTask WriteToSocketAsync(MemoryBlock memory, CancellationToken cancellation = default);

    ObjectPool<MemoryBlock> MemoryPool { get; }
        
    ObjectPool<LazyMessageProperties> MessagePropertiesPool { get; }

    ObjectPool<FrameBuilder> FrameBuilderPool { get; }
}