using System;

namespace RabbitMQ.Next.Transport.Methods
{
    public interface IMethodParser
    {
        IMethod ParseMethod(ReadOnlySpan<byte> payload);
    }

    public interface IMethodParser<out TMethod> : IMethodParser
        where TMethod : struct, IMethod
    {
        TMethod Parse(ReadOnlySpan<byte> payload);
    }
}