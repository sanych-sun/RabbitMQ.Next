using System;
using RabbitMQ.Next.Abstractions;

namespace RabbitMQ.Next.Transport.Methods.Registry
{
    internal interface IMethodRegistration
    {
        MethodId MethodId { get; }

        Type Type { get; }

        bool IsSync { get; }

        bool HasContent { get; }

        object Parser { get; }

        object Formatter { get; }
    }
}