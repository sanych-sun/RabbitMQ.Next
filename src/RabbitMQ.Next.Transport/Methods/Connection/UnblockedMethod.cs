using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Connection
{
    public readonly struct UnblockedMethod : IIncomingMethod
    {
        public uint Method => (uint) MethodId.ConnectionUnblocked;
    }
}