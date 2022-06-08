using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Next.Methods;
using RabbitMQ.Next.Buffers;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Transport;

namespace RabbitMQ.Next
{
    internal sealed class ConnectionFactory : IConnectionFactory
    {
        public static readonly IConnectionFactory Default = new ConnectionFactory();

        public async Task<IConnection> ConnectAsync(ConnectionSettings settings, IMethodRegistry registry, ISerializerFactory serializerFactory, CancellationToken cancellation)
        {
            var bufferSize = ProtocolConstants.FrameHeaderSize + settings.MaxFrameSize + 1; // 2 * (frame header + frame + frame-end) - to be sure that method and content header fit
            var memoryPool = new MemoryBlockPool(bufferSize, 200);

            var frameBuilderPool = new DefaultObjectPool<FrameBuilder>(new FrameBuilderPolicy(memoryPool), 20);

            var connection = new Connection(settings, registry, memoryPool, frameBuilderPool, serializerFactory);
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
}