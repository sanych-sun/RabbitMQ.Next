using System;

namespace RabbitMQ.Next.Transport.Methods.Registry
{
    internal interface IMethodRegistration
    {
        uint MethodId { get; }

        Type ImplementationType { get; }

        bool HasContent { get; }

        object Parser { get; }

        object Formatter { get; }
    }
}