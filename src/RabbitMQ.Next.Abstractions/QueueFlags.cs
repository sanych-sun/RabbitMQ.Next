namespace RabbitMQ.Next.Abstractions
{
    public enum QueueFlags : byte
    {
        Durable = (1 << 1),
        Exclusive = (1 << 2),
        AutoDelete = (1 << 3),
    }
}