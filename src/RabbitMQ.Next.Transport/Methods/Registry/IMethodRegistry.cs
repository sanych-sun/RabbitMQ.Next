using System;

namespace RabbitMQ.Next.Transport.Methods.Registry
{
    public interface IMethodRegistry
    {
        bool HasContent(uint methodId);

        Type GetMethodType(uint methodId);

        uint GetMethodId<TMethod>()
            where TMethod : struct, IMethod;

        IMethodFrameParser<TMethod> GetParser<TMethod>()
            where TMethod : struct, IIncomingMethod;

        IMethodFrameParser GetParser(uint methodId);

        IMethodFrameFormatter<TMethod> GetFormatter<TMethod>()
            where TMethod : struct, IOutgoingMethod;
    }
}