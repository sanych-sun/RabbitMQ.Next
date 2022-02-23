using System;

namespace RabbitMQ.Next.Methods
{
    public interface IMethodRegistry
    {
        bool HasContent(MethodId methodId);

        Type GetMethodType(MethodId methodId);

        MethodId GetMethodId<TMethod>()
            where TMethod : struct, IMethod;

        IMethodParser<TMethod> GetParser<TMethod>()
            where TMethod : struct, IIncomingMethod;

        IMethodFormatter<TMethod> GetFormatter<TMethod>()
            where TMethod : struct, IOutgoingMethod;
    }
}