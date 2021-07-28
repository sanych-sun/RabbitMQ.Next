using System;

namespace RabbitMQ.Next.Abstractions.Methods
{
    public interface IMethodParser
    {
        IIncomingMethod ParseMethod(ReadOnlyMemory<byte> payload);
    }

    public interface IMethodParser<out TMethod> : IMethodParser
        where TMethod : struct, IMethod
    {
        TMethod Parse(ReadOnlyMemory<byte> payload);
    }
}