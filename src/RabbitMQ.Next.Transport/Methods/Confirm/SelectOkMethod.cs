using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Confirm
{
    public struct SelectOkMethod : IIncomingMethod
    {
        public MethodId MethodId => MethodId.ConfirmSelectOk;
    }
}