using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Confirm;

public readonly struct SelectMethod : IOutgoingMethod
{
    public MethodId MethodId => MethodId.ConfirmSelect;
}