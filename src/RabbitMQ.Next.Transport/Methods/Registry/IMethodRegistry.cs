using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods.Registry
{
    public interface IMethodRegistry
    {
        bool HasContent(uint methodId);

        Type GetMethodType(uint methodId);

        uint GetMethodId<TMethod>()
            where TMethod : struct, IMethod;

        IMethodParser<TMethod> GetParser<TMethod>()
            where TMethod : struct, IIncomingMethod;

        IMethodParser GetParser(uint methodId);

        IMethodFormatter<TMethod> GetFormatter<TMethod>()
            where TMethod : struct, IOutgoingMethod;
    }
}