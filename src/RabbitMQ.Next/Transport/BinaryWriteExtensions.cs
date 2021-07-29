using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RabbitMQ.Next.Transport
{
    public static class BinaryWriteExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<byte> Write(this Memory<byte> target, byte data)
        {
            target.Span[0] = data;
            return target[sizeof(byte)..];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<byte> Write(this Memory<byte> target, sbyte data)
        {
            target.Span[0] = (byte)data;
            return target[sizeof(sbyte)..];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<byte> Write(this Memory<byte> target, bool data)
        {
            target.Span[0] = data ? (byte) 1 : (byte) 0;
            return target[sizeof(byte)..];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<byte> Write(this Memory<byte> target, ushort data)
        {
            BinaryPrimitives.WriteUInt16BigEndian(target.Span, data);
            return target.Slice(sizeof(ushort));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<byte> Write(this Memory<byte> target, short data)
        {
            BinaryPrimitives.WriteInt16BigEndian(target.Span, data);
            return target[sizeof(short)..];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<byte> Write(this Memory<byte> target, uint data)
        {
            BinaryPrimitives.WriteUInt32BigEndian(target.Span, data);
            return target[sizeof(uint)..];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<byte> Write(this Memory<byte> target, int data)
        {
            BinaryPrimitives.WriteInt32BigEndian(target.Span, data);
            return target[sizeof(int)..];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<byte> Write(this Memory<byte> target, ulong data)
        {
            BinaryPrimitives.WriteUInt64BigEndian(target.Span, data);
            return target[sizeof(ulong)..];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<byte> Write(this Memory<byte> target, long data)
        {
            BinaryPrimitives.WriteInt64BigEndian(target.Span, data);
            return target[sizeof(long)..];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<byte> Write(this Memory<byte> target, float data)
        {
            MemoryMarshal.Write(target.Span, ref data);
            return target[sizeof(float)..];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<byte> Write(this Memory<byte> target, decimal data)
        {
            MemoryMarshal.Write(target.Span, ref data);
            return target[sizeof(decimal)..];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<byte> Write(this Memory<byte> target, double data)
        {
            MemoryMarshal.Write(target.Span, ref data);
            return target[sizeof(double)..];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<byte> Write(this Memory<byte> target, ReadOnlyMemory<byte> data)
        {
            target = target.Write((uint)data.Length);
            data.CopyTo(target);
            return target[data.Length..];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<byte> Write(this Memory<byte> target, string data, bool isLongString = false)
        {
            var lengthMemory = target;

            target = target.Slice(isLongString ? sizeof(uint) : sizeof(byte));

            var bytesWritten = TextEncoding.GetBytes(data, target.Span);
            target = target.Slice(bytesWritten);

            if (isLongString)
            {
                lengthMemory.Write((uint) bytesWritten);
            }
            else
            {
                if (bytesWritten > 255)
                {
                    throw new ArgumentException("Short string should be less then 256 bytes", nameof(data));
                }

                lengthMemory.Write((byte) bytesWritten);
            }

            return target;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<byte> Write(this Memory<byte> target, DateTimeOffset data)
        {
            var timestamp = data.ToUnixTimeSeconds();
            return target.Write(timestamp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<byte> WriteField(this Memory<byte> target, object value)
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
        public static Memory<byte> Write(this Memory<byte> target, IEnumerable<KeyValuePair<string, object>> value)
        {
            if (value == null)
            {
                return target.Write((uint)0);
            }

            var lenPosition = target;

            target = target[sizeof(uint)..];
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
        public static Memory<byte> Write(this Memory<byte> target, object[] value)
        {
            if (value == null)
            {
                return target.Write((uint)0);
            }

            var lenPosition = target;

            target = target[sizeof(uint)..];
            var before = target.Length;

            for (var i = 0; i < value.Length; i++)
            {
                target = target.WriteField(value[i]);
            }

            var tableLen = (uint)(before - target.Length);
            lenPosition.Write(tableLen);

            return target;
        }
    }
}