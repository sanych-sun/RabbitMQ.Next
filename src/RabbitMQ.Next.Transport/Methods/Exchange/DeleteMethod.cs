using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Exchange
{
    public readonly struct DeleteMethod : IOutgoingMethod
    {
        public DeleteMethod(string exchange, bool unusedOnly)
        {
            this.Exchange = exchange;
            this.UnusedOnly = unusedOnly;
        }

        public uint Method => (uint) MethodId.ExchangeDelete;

        public string Exchange { get; }

        public bool UnusedOnly { get; }
    }
}