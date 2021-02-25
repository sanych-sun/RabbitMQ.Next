namespace RabbitMQ.Next.Abstractions.Channels
{
    public enum ChannelFrameType
    {
        Unknown = 0,
        Method = 1,
        ContentHeader = 2,
        ContentBody = 3,
    }
}