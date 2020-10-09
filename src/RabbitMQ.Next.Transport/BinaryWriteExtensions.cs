using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace RabbitMQ.Next.Transport
{
    public static class BinaryWriteExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> Write(this Span<byte> target, byte data)
        {
            var buffer = target.Slice(0, sizeof(byte));
            buffer[0] = data;
            return target.Slice(sizeof(byte));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> Write(this Span<byte> target, sbyte data)
        {
            var buffer = target.Slice(0, sizeof(sbyte));
            buffer[0] = (byte)data;
            return target.Slice(sizeof(sbyte));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> Write(this Span<byte> target, bool data)
        {
            var buffer = target.Slice(0, sizeof(byte));
            buffer[0] = data ? (byte) 1 : (byte) 0;
            return target.Slice(sizeof(byte));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> Write(this Span<byte> target, ushort data)
        {
            var buffer = target.Slice(0, sizeof(ushort));
            BinaryPrimitives.WriteUInt16BigEndian(buffer, data);
            return target.Slice(sizeof(ushort));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> Write(this Span<byte> target, short data)
        {
            var buffer = target.Slice(0, sizeof(short));
            BinaryPrimitives.WriteInt16BigEndian(buffer, data);
            return target.Slice(sizeof(short));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> Write(this Span<byte> target, uint data)
        {
            var buffer = target.Slice(0, sizeof(uint));
            BinaryPrimitives.WriteUInt32BigEndian(buffer, data);
            return target.Slice(sizeof(uint));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> Write(this Span<byte> target, int data)
        {
            var buffer = target.Slice(0, sizeof(int));
            BinaryPrimitives.WriteInt32BigEndian(buffer, data);
            return target.Slice(sizeof(int));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> Write(this Span<byte> target, ulong data)
        {
            var buffer = target.Slice(0, sizeof(ulong));
            BinaryPrimitives.WriteUInt64BigEndian(buffer, data);
            return target.Slice(sizeof(ulong));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> Write(this Span<byte> target, long data)
        {
            var buffer = target.Slice(0, sizeof(long));
            BinaryPrimitives.WriteInt64BigEndian(buffer, data);
            return target.Slice(sizeof(long));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> Write(this Span<byte> target, float data)
        {
            var buffer = target.Slice(0, sizeof(float));
            MemoryMarshal.Write(buffer, ref data);
            return target.Slice(sizeof(float));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> Write(this Span<byte> target, decimal data)
        {
            var buffer = target.Slice(0, sizeof(decimal));
            MemoryMarshal.Write(buffer, ref data);
            return target.Slice(sizeof(decimal));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> Write(this Span<byte> target, double data)
        {
            var buffer = target.Slice(0, sizeof(double));
            MemoryMarshal.Write(buffer, ref data);
            return target.Slice(sizeof(double));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> Write(this Span<byte> target, ReadOnlySpan<byte> data)
        {
            target = target.Write((uint)data.Length);
            data.CopyTo(target);
            target.Slice(data.Length);
            return target;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> Write(this Span<byte> target, string data, bool isLongString = false)
        {
            var lengthSpan = target;

            if (isLongString)
            {
                target = target.Slice(sizeof(uint));
            }
            else
            {
                if (data.Length > 255)
                {
                    throw new ArgumentException("Short string should be less then 256 characters", nameof(data));
                }
                target = target.Slice(sizeof(byte));
            }

            var bytesWritten = Encoding.UTF8.GetBytes(data, target);
            target = target.Slice(bytesWritten);

            if (isLongString)
            {
                lengthSpan.Write((uint) bytesWritten);
            }
            else
            {
                lengthSpan.Write((byte) bytesWritten);
            }

            return target;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> Write(this Span<byte> target, DateTimeOffset data)
        {
            var timestamp = data.ToUnixTimeSeconds();
            return target.Write(timestamp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> WriteField(this Span<byte> target, object value)
        {
            switch (value)
            {
                case bool boolValue:
                    target = target.Write(FieldTypePrefix.Boolean);
                    target = target.Write(boolValue);
                    break;
                case byte byteValue:
                    target = target.Write(FieldTypePrefix.Byte);
                    target = target.Write(byteValue);
                    break;
                case sbyte sbyteValue:
                    target = target.Write(FieldTypePrefix.SByte);
                    target = target.Write(sbyteValue);
                    break;
                case short shortValue:
                    target = target.Write(FieldTypePrefix.Short);
                    target = target.Write(shortValue);
                    break;
                case uint uintValue:
                    target = target.Write(FieldTypePrefix.UInt);
                    target = target.Write(uintValue);
                    break;
                case int intValue:
                    target = target.Write(FieldTypePrefix.Int);
                    target = target.Write(intValue);
                    break;
                case long longValue:
                    target = target.Write(FieldTypePrefix.Long);
                    target = target.Write(longValue);
                    break;
                case float floatValue:
                    target = target.Write(FieldTypePrefix.Single);
                    target = target.Write(floatValue);
                    break;
                case decimal decimalValue:
                    target = target.Write(FieldTypePrefix.Decimal);
                    target = target.Write(decimalValue);
                    break;
                case double doubleValue:
                    target = target.Write(FieldTypePrefix.Double);
                    target = target.Write(doubleValue);
                    break;
                case DateTimeOffset dateValue:
                    target = target.Write(FieldTypePrefix.Timestamp);
                    target = target.Write(dateValue);
                    break;
                case string stringValue:
                    target = target.Write(FieldTypePrefix.String);
                    target = target.Write(stringValue, true);
                    break;
                case object[] arrayValue:
                    target = target.Write(FieldTypePrefix.Array);
                    target = target.Write(arrayValue);
                    break;
                case IReadOnlyDictionary<string, object> tableValue:
                    target = target.Write(FieldTypePrefix.Table);
                    target = target.Write(tableValue);
                    break;
                case byte[] binaryValue:
                    target = target.Write(FieldTypePrefix.Binary);
                    target = target.Write(binaryValue);
                    break;
                case null:
                    target = target.Write(FieldTypePrefix.Null);
                    break;
                default:
                    throw new NotSupportedException($"Not supported type: {value.GetType().FullName}");
            }

            return target;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> Write(this Span<byte> target, IReadOnlyDictionary<string, object> value)
        {
            if (value == null)
            {
                return target.Write((uint)0);
            }

            var lenPosition = target;

            target = target.Slice(sizeof(uint));
            var before = target.Length;
            foreach (var item in value)
            {
                target = target.Write(item.Key);
                target = target.WriteField(item.Value);
            }

            var tableLen = (uint)(before - target.Length);
            lenPosition.Write(tableLen);

            return target;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> Write(this Span<byte> target, object[] value)
        {
            if (value == null)
            {
                return target.Write((uint)0);
            }

            var lenPosition = target;

            target = target.Slice(sizeof(uint));
            var before = target.Length;

            for (var i = 0; i < value.Length; i++)
            {
                target = target.WriteField(value[i]);
            }

            var tableLen = (uint)(before - target.Length);
            lenPosition.Write(tableLen);

            return target;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> WriteBits(
            this Span<byte> target,
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

            return target.Write(bits);
        }
    }
}