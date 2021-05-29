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

        public uint Method => (uint) MethodId.ConfirmSelect;

        public bool NoWait { get; }
    }
}