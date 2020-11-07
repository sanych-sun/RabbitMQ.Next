using System;

namespace RabbitMQ.Next.MessagePublisher.Abstractions
{
    [Flags]
    public enum PublishOptions : byte
    {
        None = 0,
        Mandatory = 1 << 0,
        Immediate = 1 << 1,
    }
}