using System;

namespace RabbitMQ.Next.Channels
{
    [Flags]
    public enum PublishFlags : byte
    {
        None = 0,
        Mandatory = 1 << 0,
        Immediate = 1 << 1,
    }
}