namespace RabbitMQ.Next.Abstractions
{
    public enum ConnectionState
    {
        Pending = 0,
        Connecting = 1,
        Configuring = 2,
        Open = 3,
        Blocked = 4,
        Broken = 5,
        Closed = 6,
    }
}