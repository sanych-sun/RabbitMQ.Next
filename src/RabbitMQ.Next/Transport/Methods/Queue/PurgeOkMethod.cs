using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Queue
{
    public readonly struct PurgeOkMethod : IIncomingMethod
    {
        public PurgeOkMethod(uint messageCount)
        {
            this.MessageCount = messageCount;
        }

        public MethodId MethodId => MethodId.QueuePurgeOk;

        public uint MessageCount { get; }
    }
}