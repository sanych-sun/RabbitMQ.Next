using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic
{
    public readonly struct CancelOkMethod : IIncomingMethod
    {
        public CancelOkMethod(string consumerTag)
        {
            this.ConsumerTag = consumerTag;
        }

        public uint Method => (uint) MethodId.BasicCancelOk;

        public string ConsumerTag { get; }
    }
}