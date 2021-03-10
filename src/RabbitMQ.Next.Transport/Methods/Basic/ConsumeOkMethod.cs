using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic
{
    public readonly struct ConsumeOkMethod : IIncomingMethod
    {
        public ConsumeOkMethod(string consumerTag)
        {
            this.ConsumerTag = consumerTag;
        }

        public uint Method => (uint) MethodId.BasicConsumeOk;

        public string ConsumerTag { get; }
    }
}