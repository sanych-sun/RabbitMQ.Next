using System.Threading.Channels;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Next.Buffers;
using RabbitMQ.Next.Channels;

namespace RabbitMQ.Next
{
    internal interface IConnectionInternal : IConnection
    {
        ChannelWriter<MemoryBlock> SocketWriter { get; }

        ObjectPool<MemoryBlock> MemoryPool { get; }

        ObjectPool<FrameBuilder> FrameBuilderPool { get; }
    }
}