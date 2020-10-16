using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Queue
{
    public readonly struct BindOkMethod : IIncomingMethod
    {
        public uint Method => (uint) MethodId.QueueBindOk;
    }
}