namespace RabbitMQ.Next.Transport.Methods.Connection
{
    public readonly struct OpenOkMethod : IIncomingMethod
    {
        public uint Method => (uint) MethodId.ConnectionOpenOk;
    }
}