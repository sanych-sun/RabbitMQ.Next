using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic
{
    public readonly struct CancelMethod : IOutgoingMethod
    {
        public CancelMethod(string consumerTag, bool noWait)
        {
            this.ConsumerTag = consumerTag;
            this.NoWait = noWait;
        }

        public MethodId MethodId => MethodId.BasicCancel;

        public string ConsumerTag { get; }

        public bool NoWait { get; }
    }
}