namespace RabbitMQ.Next.MessagePublisher
{
    public struct PublisherChannelOptions
    {
        public string Exchange { get; set; }

        public int localQueueLimit { get; set; }


    }
}