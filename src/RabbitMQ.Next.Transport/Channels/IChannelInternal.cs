using System;
using RabbitMQ.Next.Abstractions.Channels;

namespace RabbitMQ.Next.Transport.Channels
{
    internal interface IChannelInternal : IChannel
    {
        public ushort ChannelNumber { get; }

        public ChannelWriter Writer { get; }

        public void SetCompleted(Exception ex = null);
    }
}