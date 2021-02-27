using System;

namespace RabbitMQ.Next.Consumer.Abstractions
{
    [Flags]
    public enum UnhandledMessageMode
    {
        Drop = 0,
        Requeue = 1 << 0,
        StopConsumer = 1 << 1,
    }
}