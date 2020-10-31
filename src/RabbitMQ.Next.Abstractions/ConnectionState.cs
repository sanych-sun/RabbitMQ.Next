namespace RabbitMQ.Next.Abstractions
{
    public enum ConnectionState
    {
        Pending = 0,
        Connecting = 1,
        Negotiating = 2,
        Configuring = 3,
        Open = 4,
        Blocked = 5,
        Closed = 6,
        Broken = 7,
    }
}