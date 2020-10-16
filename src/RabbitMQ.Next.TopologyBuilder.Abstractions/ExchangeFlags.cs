using System;

namespace RabbitMQ.Next.TopologyBuilder.Abstractions
{
    [Flags]
    public enum ExchangeFlags : byte
    {
        Durable = (1 << 1),
        AutoDelete = (1 << 2),
        Internal = (1 << 3),
    }
}