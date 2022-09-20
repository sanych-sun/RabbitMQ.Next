using System;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods;

internal interface IMethodParser<out TMethod>
    where TMethod : struct, IMethod
{
    TMethod Parse(ReadOnlySpan<byte> payload);
}