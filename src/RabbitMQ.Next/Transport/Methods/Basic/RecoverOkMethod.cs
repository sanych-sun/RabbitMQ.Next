using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic
{
    public readonly struct RecoverOkMethod : IIncomingMethod
    {
        public MethodId MethodId => MethodId.BasicRecoverOk;
    }
}