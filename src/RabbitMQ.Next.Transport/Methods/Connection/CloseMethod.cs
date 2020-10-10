namespace RabbitMQ.Next.Transport.Methods.Connection
{
    public readonly struct CloseMethod : IIncomingMethod, IOutgoingMethod
    {
        public uint Method => (uint) MethodId.ConnectionClose;

        public CloseMethod(ushort statusCode, string description, uint failedMethod)
        {
            this.StatusCode = statusCode;
            this.Description = description;
            this.FailedMethodUid = failedMethod;
        }

        public ushort StatusCode { get; }

        public string Description { get; }

        public uint FailedMethodUid { get; }
    }
}