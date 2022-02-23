using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic
{
    public readonly struct RecoverOkMethod : IIncomingMethod
    {
        public MethodId MethodId => MethodId.BasicRecoverOk;
    }
}