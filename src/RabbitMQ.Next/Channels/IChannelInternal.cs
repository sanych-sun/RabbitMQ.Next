using System;
using System.IO.Pipelines;
using RabbitMQ.Next.Abstractions.Channels;

namespace RabbitMQ.Next.Channels
{
    internal interface IChannelInternal : IChannel
    {
        public PipeWriter Writer { get; }

        public void SetCompleted(Exception ex = null);
    }
}