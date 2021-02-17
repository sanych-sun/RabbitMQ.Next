namespace RabbitMQ.Next.MessagePublisher
{
    public class PublisherChannelOptions
    {
        public static readonly PublisherChannelOptions Default = new PublisherChannelOptions(10);

        public PublisherChannelOptions(int localQueueLimit)
        {
            this.LocalQueueLimit = localQueueLimit;
        }

        public int LocalQueueLimit { get; }
    }
}