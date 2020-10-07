namespace RabbitMQ.Next.Transport.Methods.Exchange
{
    public readonly struct UnbindOkMethod : IIncomingMethod
    {
        public uint Method => (uint) MethodId.ExchangeUnbindOk;
    }
}