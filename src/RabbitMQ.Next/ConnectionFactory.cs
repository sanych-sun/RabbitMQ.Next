using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Next.Buffers;
using RabbitMQ.Next.Transport;

namespace RabbitMQ.Next;

internal sealed class ConnectionFactory : IConnectionFactory
{
    public static readonly IConnectionFactory Default = new ConnectionFactory();

    public async Task<IConnection> ConnectAsync(ConnectionSettings settings, CancellationToken cancellation = default)
    {
        // for best performance and code simplification buffer should fit entire frame
        // (frame header + frame payload + frame-end)
        var bufferSize = ProtocolConstants.FrameHeaderSize + settings.MaxFrameSize + ProtocolConstants.FrameEndSize;
        var memoryPool = new DefaultObjectPool<byte[]>(new MemoryPoolPolicy(bufferSize), 100);
        var memoryPool2 = new MemoryBlockPool(bufferSize, 100);
        
        var connection = new Connection(settings, memoryPool2, memoryPool);
        await connection.OpenConnectionAsync(cancellation);
        return connection;
    }
}