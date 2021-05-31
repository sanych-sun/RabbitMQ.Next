using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Tests.Transport.Methods.Registry
{
    public readonly struct DummyMethod<TData> : IMethod, IIncomingMethod, IOutgoingMethod
    {
        public DummyMethod(MethodId methodId, TData data)
        {
            this.MethodId = methodId;
            this.Data = data;
        }

        public MethodId MethodId { get; }

        public TData Data { get; }
    }
}