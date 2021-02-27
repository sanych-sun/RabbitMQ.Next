namespace RabbitMQ.Next.Consumer.Abstractions
{
    public enum AcknowledgementMode
    {
        AutoEachMessage = 0,
        AutoMultiple = 1,
        Manual = 2,
        NoAck = 3,
    }
}