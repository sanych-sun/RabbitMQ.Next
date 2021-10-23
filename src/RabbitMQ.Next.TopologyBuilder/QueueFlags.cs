using System;

namespace RabbitMQ.Next.TopologyBuilder
{
    [Flags]
    public enum QueueFlags : byte
    {
        None = 0,
        Durable = (1 << 1),
        Exclusive = (1 << 2),
        AutoDelete = (1 << 3),
    }
}