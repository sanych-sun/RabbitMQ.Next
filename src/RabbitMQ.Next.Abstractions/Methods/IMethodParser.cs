using System;

namespace RabbitMQ.Next.Methods
{
    public interface IMethodParser<out TMethod>
        where TMethod : struct, IMethod
    {
        TMethod Parse(ReadOnlySpan<byte> payload);
    }
}