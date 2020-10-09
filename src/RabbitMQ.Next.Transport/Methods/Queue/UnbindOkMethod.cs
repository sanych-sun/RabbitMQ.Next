namespace RabbitMQ.Next.Transport.Methods.Queue
{
    public readonly struct UnbindOkMethod : IIncomingMethod
    {
        public uint Method => (uint) MethodId.QueueUnbindOk;
    }
}