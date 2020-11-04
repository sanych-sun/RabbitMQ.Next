using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Tests.Transport.Methods.Registry
{
    public readonly struct DummyMethod<TData> : IMethod, IIncomingMethod, IOutgoingMethod
    {
        public DummyMethod(uint methodId, TData data)
        {
            this.Method = methodId;
            this.Data = data;
        }

        public uint Method { get; }

        public TData Data { get; }
    }
}