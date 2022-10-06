using System;
using System.Threading.Channels;
using RabbitMQ.Next.Buffers;
using RabbitMQ.Next.Transport;

namespace RabbitMQ.Next.Channels;

internal interface IChannelInternal : IChannel
{
    public ChannelWriter<(FrameType Type, MemoryBlock Payload)> FrameWriter { get; }

    public bool TryComplete(Exception ex = null);

    public ushort ChannelNumber { get; }
}