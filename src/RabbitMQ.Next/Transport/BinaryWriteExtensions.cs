using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RabbitMQ.Next.Transport;

internal static class BinaryWriteExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<byte> Write(this Span<byte> target, byte data)
    {
        target[0] = data;
        return target.Slice(sizeof(byte));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<byte> Write(this Span<byte> target, sbyte data)
    {
        target[0] = (byte)data;
        return target.Slice(sizeof(sbyte));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<byte> Write(this Span<byte> target, bool data)
    {
        target[0] = data ? (byte) 1 : (byte) 0;
        return target.Slice(sizeof(byte));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<byte> Write(this Span<byte> target, ushort data)
    {
        BinaryPrimitives.WriteUInt16BigEndian(target, data);
        return target.Slice(sizeof(ushort));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<byte> Write(this Span<byte> target, short data)
    {
        BinaryPrimitives.WriteInt16BigEndian(target, data);
        return target.Slice(sizeof(short));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<byte> Write(this Span<byte> target, uint data)
    {
        BinaryPrimitives.WriteUInt32BigEndian(target, data);
        return target.Slice(sizeof(uint));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<byte> Write(this Span<byte> target, int data)
    {
        BinaryPrimitives.WriteInt32BigEndian(target, data);
        return target.Slice(sizeof(int));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<byte> Write(this Span<byte> target, ulong data)
    {
        BinaryPrimitives.WriteUInt64BigEndian(target, data);
        return target.Slice(sizeof(ulong));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<byte> Write(this Span<byte> target, long data)
    {
        BinaryPrimitives.WriteInt64BigEndian(target, data);
        return target.Slice(sizeof(long));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<byte> Write(this Span<byte> target, float data)
    {
        MemoryMarshal.Write(target, ref data);
        return target.Slice(sizeof(float));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<byte> Write(this Span<byte> target, decimal data)
    {
        MemoryMarshal.Write(target, ref data);
        return target.Slice(sizeof(decimal));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<byte> Write(this Span<byte> target, double data)
    {
        MemoryMarshal.Write(target, ref data);
        return target.Slice(sizeof(double));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<byte> Write(this Span<byte> target, ReadOnlySpan<byte> data)
    {
        target = target.Write((uint)data.Length);
        data.CopyTo(target);
        return target.Slice(data.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<byte> Write(this Span<byte> target, string data, bool isLongString = false)
    {
        var lengthMemory = target;

        target = target.Slice(isLongString ? sizeof(uint) : sizeof(byte));

        var bytesWritten = TextEncoding.GetBytes(data, target);
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
    public static Span<byte> Write(this Span<byte> target, IEnumerable<KeyValuePair<string, object>> value)
    {
        if (value == null)
        {
            return target.Write((uint)0);
        }

        var lenPosition = target;

        target = target[sizeof(uint)..];
        var before = target.Length;
        foreach (var (key, o) in value)
        {
            target = target.Write(key);
            target = target.WriteField(o);
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
}
