using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic
{
    public readonly struct NackMethod : IOutgoingMethod
    {
        public NackMethod(ulong deliveryTag, bool multiple, bool requeue)
        {
            this.DeliveryTag = deliveryTag;
            this.Multiple = multiple;
            this.Requeue = requeue;
        }

        public uint Method => (uint) MethodId.BasicNack;

        public ulong DeliveryTag { get; }

        public bool Multiple { get; }

        public bool Requeue { get; }
    }
}