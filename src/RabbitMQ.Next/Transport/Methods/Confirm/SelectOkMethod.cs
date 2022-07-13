using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Confirm;

public struct SelectOkMethod : IIncomingMethod
{
    public MethodId MethodId => MethodId.ConfirmSelectOk;
}