using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Connection
{
    public readonly struct CloseOkMethod : IIncomingMethod, IOutgoingMethod
    {
        public uint Method => (uint) MethodId.ConnectionCloseOk;
    }
}