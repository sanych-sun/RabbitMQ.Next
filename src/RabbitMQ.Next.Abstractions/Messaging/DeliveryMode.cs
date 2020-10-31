namespace RabbitMQ.Next.Abstractions.Messaging
{
    public enum DeliveryMode : byte
    {
        Unset = 0,
        NonPersistent = 1,
        Persistent = 2,
    }
}