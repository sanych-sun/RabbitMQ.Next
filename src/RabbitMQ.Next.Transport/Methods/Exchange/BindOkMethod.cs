namespace RabbitMQ.Next.Transport.Methods.Exchange
{
    public readonly struct BindOkMethod : IIncomingMethod
    {
        public uint Method => (uint) MethodId.ExchangeBindOk;
    }
}