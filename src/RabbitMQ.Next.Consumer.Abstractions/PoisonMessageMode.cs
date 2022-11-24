namespace RabbitMQ.Next.Consumer;

public enum PoisonMessageMode
{
    Drop = 0,
    Requeue = 1,
}