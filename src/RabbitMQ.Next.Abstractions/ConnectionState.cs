namespace RabbitMQ.Next.Abstractions
{
    public enum ConnectionState
    {
        Pending = 0,
        Connecting,
        Negotiating,
        Open,
        Closed,
        Broken,
    }
}