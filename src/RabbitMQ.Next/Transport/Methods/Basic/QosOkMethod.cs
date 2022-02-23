using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic
{
    public readonly struct QosOkMethod : IIncomingMethod
    {
        public MethodId MethodId => MethodId.BasicQosOk;
    }
}