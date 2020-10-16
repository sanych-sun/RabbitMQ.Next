using System;

namespace RabbitMQ.Next.Abstractions.Exceptions
{
    public abstract class ProtocolException : Exception
    {
        protected ProtocolException(string message)
            : base(message)
        {}
    }
}