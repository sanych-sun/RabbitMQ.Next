namespace RabbitMQ.Next.Abstractions.Exceptions
{
    public class ConnectionException : ProtocolException
    {
        public ConnectionException(ushort errorCode, string message)
            : base(message)
        {
            this.ErrorCode = errorCode;
        }

        public ushort ErrorCode { get; }
    }
}