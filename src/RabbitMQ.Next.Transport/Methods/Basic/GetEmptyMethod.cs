using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic
{
    public readonly struct GetEmptyMethod : IIncomingMethod
    {
        public uint Method => (uint) MethodId.BasicGetEmpty;
    }
}