using System;
using System.Runtime.CompilerServices;

namespace RabbitMQ.Next.Transport
{
    public static class BitConverter
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ComposeFlags(
            bool bit1, bool bit2 = false, bool bit3 = false, bool bit4 = false,
            bool bit5 = false, bool bit6 = false, bool bit7 = false, bool bit8 = false)
        {
            var bits = Convert.ToByte(bit1);
            bits |= (byte)(Convert.ToByte(bit2) << 1);
            bits |= (byte)(Convert.ToByte(bit3) << 2);
            bits |= (byte)(Convert.ToByte(bit4) << 3);
            bits |= (byte)(Convert.ToByte(bit5) << 4);
            bits |= (byte)(Convert.ToByte(bit6) << 5);
            bits |= (byte)(Convert.ToByte(bit7) << 6);
            bits |= (byte)(Convert.ToByte(bit8) << 7);

            return bits;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFlagSet(long flags, byte bitPosition)
            => (flags & (1 << bitPosition)) != 0;
    }
}