using System;

namespace RabbitMQ.Next.TopologyBuilder.Abstractions.Exceptions
{
    public class ConflictException : Exception
    {
        public ConflictException(string message, Exception innerException = null)
            : base(message, innerException)
        {}
    }
}