namespace RabbitMQ.Next.Transport
{
    internal enum FrameType : byte
    {
        Malformed = 0,
        Method = 1,
        ContentHeader = 2,
        ContentBody = 3,
        Heartbeat = 8,
    }
}