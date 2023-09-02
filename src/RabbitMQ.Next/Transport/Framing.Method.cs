using System;
using System.Runtime.CompilerServices;
using RabbitMQ.Next.Methods;
using RabbitMQ.Next.Transport.Methods;

namespace RabbitMQ.Next.Transport;

internal static partial class Framing
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<byte> WriteMethodArgs<TMethod>(this Span<byte> buffer, TMethod method)
        where TMethod : struct, IOutgoingMethod
    {
        var formatter = MethodRegistry.GetFormatter<TMethod>();
        
        buffer = buffer.Write((uint)method.MethodId);
        return formatter.Write(buffer, method);
    }
}