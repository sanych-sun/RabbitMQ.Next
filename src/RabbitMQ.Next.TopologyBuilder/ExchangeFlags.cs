using System;

namespace RabbitMQ.Next.TopologyBuilder
{
    [Flags]
    public enum ExchangeFlags : byte
    {
        None = 0,
        Durable = (1 << 1),
        AutoDelete = (1 << 2),
        Internal = (1 << 3),
    }
}