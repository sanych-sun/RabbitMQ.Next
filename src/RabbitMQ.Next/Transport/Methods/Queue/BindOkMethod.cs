using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Queue;

public readonly struct BindOkMethod : IIncomingMethod
{
    public MethodId MethodId => MethodId.QueueBindOk;
}