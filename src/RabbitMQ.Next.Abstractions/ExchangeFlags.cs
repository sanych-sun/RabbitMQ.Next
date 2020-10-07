using System;

namespace RabbitMQ.Next.Abstractions
{
    [Flags]
    public enum ExchangeFlags : byte
    {
        Passive = (1 << 0),
        Durable = (1 << 1),
        AutoDelete = (1 << 2),
        Internal = (1 << 3),
    }
}