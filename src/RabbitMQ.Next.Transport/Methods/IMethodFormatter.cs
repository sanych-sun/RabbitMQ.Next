using System;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods
{
    public interface IMethodFormatter<in TMethod>
        where TMethod : struct, IMethod
    {
        Span<byte> Write(Span<byte> destination, TMethod method);
    }
}