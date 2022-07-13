using System;

namespace RabbitMQ.Next.Methods;

public interface IMethodFormatter<in TMethod>
    where TMethod : struct, IMethod
{
    int Write(Span<byte> destination, TMethod method);
}