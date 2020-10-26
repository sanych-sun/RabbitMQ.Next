namespace RabbitMQ.Next.Abstractions
{
    public enum ConnectionState
    {
        Pending = 0,
        Connecting = 1,
        Negotiating = 2,
        Open = 3,
        Blocked = 4,
        Closed = 5,
        Broken = 6,
    }
}