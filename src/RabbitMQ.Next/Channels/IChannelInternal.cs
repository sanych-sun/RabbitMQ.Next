using System;
using RabbitMQ.Next.Buffers;
using RabbitMQ.Next.Transport;

namespace RabbitMQ.Next.Channels;

internal interface IChannelInternal : IChannel
{
    public void PushFrame(FrameType type, MemoryBlock payload);

    public bool TryComplete(Exception ex = null);

    public ushort ChannelNumber { get; }
}