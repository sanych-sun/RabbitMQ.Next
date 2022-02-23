namespace RabbitMQ.Next.Messaging
{
    public enum DeliveryMode : byte
    {
        Unset = 0,
        NonPersistent = 1,
        Persistent = 2,
    }
}