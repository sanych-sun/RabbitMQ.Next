namespace RabbitMQ.Next.Transport.Channels
{
    public enum ContentFrameHandlerState
    {
        None = 0,
        ExpectHeader = 1,
        ExpectBody = 2,
    }
}