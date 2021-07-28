using System;
using System.Threading.Channels;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Buffers;

namespace RabbitMQ.Next.Channels
{
    internal interface IChannelInternal : IChannel
    {
        public ChannelWriter<(FrameType Type, MemoryBlock Payload)> FrameWriter { get; }

        public void SetCompleted(Exception ex = null);
    }
}