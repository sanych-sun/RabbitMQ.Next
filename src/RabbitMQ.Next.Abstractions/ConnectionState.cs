namespace RabbitMQ.Next;

public enum ConnectionState
{
    Closed = 0,
    Connecting = 1,
    Negotiating = 2,
    Configuring = 3,
    Open = 4,
    Broken = 5,
}