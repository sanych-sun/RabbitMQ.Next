using System;

namespace RabbitMQ.Next.Abstractions.Methods
{
    public interface IMethodParser<out TMethod>
        where TMethod : struct, IMethod
    {
        TMethod Parse(ReadOnlySpan<byte> payload);
    }
}