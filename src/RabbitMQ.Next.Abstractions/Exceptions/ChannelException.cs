namespace RabbitMQ.Next.Abstractions.Exceptions
{
    public class ChannelException : ProtocolException
    {
        public ChannelException(ushort errorCode, string message, MethodId failedMethodId)
            : base(message)
        {
            this.ErrorCode = errorCode;
            this.FailedMethodId = failedMethodId;
        }

        public ushort ErrorCode { get; }

        public MethodId FailedMethodId { get; }
    }
}