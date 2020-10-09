namespace RabbitMQ.Next.Transport.Methods.Queue
{
    public readonly struct PurgeOkMethod : IIncomingMethod
    {
        public PurgeOkMethod(uint messageCount)
        {
            this.MessageCount = messageCount;
        }

        public uint Method => (uint) MethodId.QueuePurgeOk;

        public uint MessageCount { get; }
    }
}