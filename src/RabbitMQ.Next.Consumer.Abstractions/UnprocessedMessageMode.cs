using System;

namespace RabbitMQ.Next.Consumer.Abstractions
{
    [Flags]
    public enum UnprocessedMessageMode
    {
        Drop = 0,
        Requeue = 1 << 0,
        StopConsumer = 1 << 1,

        Default = Requeue | StopConsumer,
    }
}