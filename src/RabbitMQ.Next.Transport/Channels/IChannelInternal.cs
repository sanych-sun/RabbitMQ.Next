using System;
using System.IO.Pipelines;
using RabbitMQ.Next.Abstractions.Channels;

namespace RabbitMQ.Next.Transport.Channels
{
    internal interface IChannelInternal : IChannel
    {
        public PipeWriter Writer { get; }

        public void SetCompleted(Exception ex = null);
    }
}