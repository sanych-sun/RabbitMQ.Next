using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Queue
{
    public readonly struct DeclareOkMethod : IIncomingMethod
    {
        public DeclareOkMethod(string queue, uint messageCount, uint consumerCount)
        {
            this.Queue = queue;
            this.MessageCount = messageCount;
            this.ConsumerCount = consumerCount;
        }

        public MethodId MethodId => MethodId.QueueDeclareOk;

        public string Queue { get; }

        public uint MessageCount { get; }

        public uint ConsumerCount { get; }
    }
}