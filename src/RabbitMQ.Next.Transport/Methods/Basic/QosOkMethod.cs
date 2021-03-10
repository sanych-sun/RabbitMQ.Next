using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic
{
    public readonly struct QosOkMethod : IIncomingMethod
    {
        public uint Method => (uint) MethodId.BasicQosOk;
    }
}