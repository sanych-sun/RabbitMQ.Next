using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Next.Methods;
using RabbitMQ.Next.Buffers;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Transport;

namespace RabbitMQ.Next
{
    internal sealed class ConnectionFactory : IConnectionFactory
    {
        public static readonly IConnectionFactory Default = new ConnectionFactory();

        public async Task<IConnection> ConnectAsync(ConnectionSettings settings, IMethodRegistry registry, CancellationToken cancellation)
        {
            var bufferSize = ProtocolConstants.FrameHeaderSize + settings.MaxFrameSize + 1; // 2 * (frame header + frame + frame-end) - to be sure that method and content header fit
            var memoryPool = new DefaultObjectPool<MemoryBlock>(new ObjectPoolPolicy<MemoryBlock>(
                () => new MemoryBlock(bufferSize),
                memory => memory.Reset()), 100);

            var frameBuilderPool = new DefaultObjectPool<FrameBuilder>(new ObjectPoolPolicy<FrameBuilder>(
                () => new FrameBuilder(memoryPool),
                builder =>
                {
                    builder.Reset();
                    return true;
                }), 200);

            var connection = new Connection(settings, registry, memoryPool, frameBuilderPool);
            await connection.OpenConnectionAsync(cancellation);
            return connection;
        }
    }
}