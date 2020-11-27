using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic
{
    public readonly struct RecoverMethod : IOutgoingMethod
    {
        public RecoverMethod(bool requeue)
        {
            this.Requeue = requeue;
        }

        public uint Method => (uint) MethodId.BasicRecover;

        public bool Requeue { get; }
    }
}