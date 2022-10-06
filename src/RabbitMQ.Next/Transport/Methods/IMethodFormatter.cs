using System;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods;

internal interface IMethodFormatter<in TMethod>
    where TMethod : struct, IMethod
{
    int Write(Span<byte> destination, TMethod method);
}