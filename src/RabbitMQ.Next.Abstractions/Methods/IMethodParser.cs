using System;

namespace RabbitMQ.Next.Abstractions.Methods
{
    public interface IMethodParser
    {
        IIncomingMethod ParseMethod(ReadOnlySpan<byte> payload);
    }

    public interface IMethodParser<out TMethod> : IMethodParser
        where TMethod : struct, IMethod
    {
        TMethod Parse(ReadOnlySpan<byte> payload);
    }
}