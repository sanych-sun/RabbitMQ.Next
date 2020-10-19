using System;

namespace RabbitMQ.Next.Abstractions.Exceptions
{
    public class ChannelException : ProtocolException
    {
        public ChannelException(ushort errorCode, string message, uint failedMethodId)
            : base(message)
        {
            this.ErrorCode = errorCode;
            this.FailedMethodId = failedMethodId;
        }

        public ushort ErrorCode { get; }

        public uint FailedMethodId { get; }
    }
}