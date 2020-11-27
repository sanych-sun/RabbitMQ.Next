using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic
{
    public readonly struct RecoverOkMethod : IIncomingMethod
    {
        public uint Method => (uint) MethodId.BasicRecoverOk;
    }
}