using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Basic
{
    public readonly struct GetMethod : IOutgoingMethod
    {
        public GetMethod(string queue, bool noAck)
        {
            this.Queue = queue;
            this.NoAck = noAck;
        }

        public uint Method => (uint) MethodId.BasicGet;

        public string Queue { get; }

        public bool NoAck { get; }
    }
}