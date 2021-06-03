using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Queue
{
    public readonly struct DeleteOkMethod : IIncomingMethod
    {
        public DeleteOkMethod(uint messageCount)
        {
            this.MessageCount = messageCount;
        }

        public MethodId MethodId => MethodId.QueueDeleteOk;

        public uint MessageCount { get; }
    }
}