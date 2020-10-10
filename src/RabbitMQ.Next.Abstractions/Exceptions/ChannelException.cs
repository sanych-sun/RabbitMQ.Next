using System;

namespace RabbitMQ.Next.Abstractions.Exceptions
{
    public class ChannelException : Exception
    {
        public ChannelException(int errorCode, string message, uint failedMethodId)
            : base(message)
        {
            this.ErrorCode = errorCode;
            this.FailedMethodId = failedMethodId;
        }

        public int ErrorCode { get; }

        public uint FailedMethodId { get; }
    }
}