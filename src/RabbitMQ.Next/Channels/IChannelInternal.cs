using System;
using System.Threading.Channels;
using RabbitMQ.Next.Buffers;

namespace RabbitMQ.Next.Channels;

internal interface IChannelInternal : IChannel
{
    public ChannelWriter<(FrameType Type, int Size, MemoryBlock Payload)> FrameWriter { get; }

    public bool TryComplete(Exception ex = null);

    public ushort ChannelNumber { get; }
}