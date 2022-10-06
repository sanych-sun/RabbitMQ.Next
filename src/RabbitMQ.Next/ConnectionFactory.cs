using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Next.Buffers;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods;
using RabbitMQ.Next.Transport.Methods.Registry;

namespace RabbitMQ.Next;

internal sealed class ConnectionFactory : IConnectionFactory
{
    public static readonly IConnectionFactory Default = new ConnectionFactory();

    public async Task<IConnection> ConnectAsync(ConnectionSettings settings, CancellationToken cancellation)
    {
        var registry = new MethodRegistryBuilder()
            .AddConnectionMethods()
            .AddChannelMethods()
            .AddExchangeMethods()
            .AddQueueMethods()
            .AddBasicMethods()
            .AddConfirmMethods()
            .Build();

        var memoryPool = new MemoryBlockPool(settings.BufferSize, 1000);

        var frameBuilderPool = new DefaultObjectPool<FrameBuilder>(new FrameBuilderPolicy(memoryPool), 10);

        var connection = new Connection(settings, registry, memoryPool, frameBuilderPool);
        await connection.OpenConnectionAsync(cancellation);
        return connection;
    }
        
    private class FrameBuilderPolicy : PooledObjectPolicy<FrameBuilder>
    {
        private readonly ObjectPool<MemoryBlock> memoryPool;

        public FrameBuilderPolicy(ObjectPool<MemoryBlock> memoryPool)
        {
            this.memoryPool = memoryPool;
        }
            
        public override FrameBuilder Create() => new (this.memoryPool);

        public override bool Return(FrameBuilder obj)
        {
            obj.Reset();
            return true;
        }
    }
}