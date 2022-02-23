using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Connection
{
    public readonly struct UnblockedMethod : IIncomingMethod
    {
        public MethodId MethodId => MethodId.ConnectionUnblocked;
    }
}