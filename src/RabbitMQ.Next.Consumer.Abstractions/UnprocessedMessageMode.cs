namespace RabbitMQ.Next.Consumer.Abstractions
{
    public enum UnprocessedMessageMode
    {
        Drop = 0,
        Requeue = 1,
    }
}