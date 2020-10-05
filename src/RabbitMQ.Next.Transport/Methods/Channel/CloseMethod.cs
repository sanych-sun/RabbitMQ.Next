namespace RabbitMQ.Next.Transport.Methods.Channel
{
    public readonly struct CloseMethod : IIncomingMethod, IOutgoingMethod
    {
        public CloseMethod(ReplyCode statusCode, string description, uint failedMethod)
        {
            this.StatusCode = statusCode;
            this.Description = description;
            this.FailedMethodUid = failedMethod;
        }

        public uint Method => (uint) MethodId.ChannelClose;

        public ReplyCode StatusCode { get; }

        public string Description { get; }

        public uint FailedMethodUid { get; }
    }
}