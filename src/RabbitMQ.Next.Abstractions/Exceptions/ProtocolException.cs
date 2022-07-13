using System;

namespace RabbitMQ.Next.Exceptions;

public abstract class ProtocolException : Exception
{
    protected ProtocolException(string message)
        : base(message)
    {}
}