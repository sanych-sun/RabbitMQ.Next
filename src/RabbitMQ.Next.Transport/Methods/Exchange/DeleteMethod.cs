namespace RabbitMQ.Next.Transport.Methods.Exchange
{
    public readonly struct DeleteMethod : IOutgoingMethod
    {
        public DeleteMethod(string name, bool unusedOnly)
        {
            this.Name = name;
            this.UnusedOnly = unusedOnly;
        }

        public uint Method => (uint) MethodId.ExchangeDelete;

        public string Name { get; }

        public bool UnusedOnly { get; }
    }
}