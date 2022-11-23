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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<byte> GetMethodId(this ReadOnlyMemory<byte> data, out MethodId methodId)
    {
        data.Span.Read(out uint method);
        methodId = (MethodId) method;

        return data[sizeof(uint)..];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TMethod ParseMethodArgs<TMethod>(this ReadOnlyMemory<byte> payload)
        where TMethod : struct, IIncomingMethod
    {
        var parser = MethodRegistry.GetParser<TMethod>();
        return parser.Parse(payload.Span);
    }
}