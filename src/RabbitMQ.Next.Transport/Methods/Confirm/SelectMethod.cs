using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Confirm
{
    public readonly struct SelectMethod : IOutgoingMethod
    {
        public SelectMethod(bool noWait)
        {
            this.NoWait = noWait;
        }

        public MethodId MethodId => MethodId.ConfirmSelect;

        public bool NoWait { get; }
    }
}