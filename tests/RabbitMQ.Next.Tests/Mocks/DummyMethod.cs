using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Tests.Mocks
{
    public readonly struct DummyMethod<TData> : IIncomingMethod, IOutgoingMethod
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