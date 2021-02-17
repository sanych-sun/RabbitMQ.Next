namespace RabbitMQ.Next.MessagePublisher
{
    public class PublisherChannelOptions
    {
        public PublisherChannelOptions(int localQueueLimit)
        {
            this.LocalQueueLimit = localQueueLimit;
        }

        public int LocalQueueLimit { get; }
    }
}