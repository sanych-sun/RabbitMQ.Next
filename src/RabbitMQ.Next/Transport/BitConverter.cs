using System;
using System.Runtime.CompilerServices;

namespace RabbitMQ.Next.Transport;

internal static class BitConverter
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsFlagSet(long flags, byte bitPosition)
        => (flags & (1 << bitPosition)) != 0;
}