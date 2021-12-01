using System;

namespace RabbitMQ.Next.Abstractions.Methods
{
    public interface IMethodFormatter<in TMethod>
        where TMethod : struct, IMethod
    {
        int Write(Span<byte> destination, TMethod method);
    }
}