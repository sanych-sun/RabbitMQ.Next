using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic
{
    public readonly struct CancelOkMethod : IIncomingMethod
    {
        public CancelOkMethod(string consumerTag)
        {
            this.ConsumerTag = consumerTag;
        }

        public MethodId MethodId => MethodId.BasicCancelOk;

        public string ConsumerTag { get; }
    }
}