namespace RabbitMQ.Next.Abstractions.Exceptions
{
    public class ConnectionException : ProtocolException
    {
        public ConnectionException(ReplyCode replyCode, string message)
            : this((ushort)replyCode, message)
        {}

        public ConnectionException(ushort replyCode, string message)
            : base(message)
        {
            this.ReplyCode = replyCode;
        }

        public ushort ReplyCode { get; }
    }
}