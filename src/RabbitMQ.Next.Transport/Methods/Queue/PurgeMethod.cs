namespace RabbitMQ.Next.Transport.Methods.Queue
{
    public readonly struct PurgeMethod : IOutgoingMethod
    {
        public PurgeMethod(string queue)
        {
            this.Queue = queue;
        }

        public uint Method => (uint) MethodId.QueuePurge;

        public string Queue { get; }
    }
}