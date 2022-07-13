using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RabbitMQ.Next.Transport;

public static class BinaryReadExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<byte> Read(this ReadOnlySpan<byte> source, out byte result)
    {
        result = source[0];
        return source[sizeof(byte)..];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<byte> Read(this ReadOnlySpan<byte> source, out sbyte result)
    {
        result = (sbyte)source[0];
        return source[sizeof(sbyte)..];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<byte> Read(this ReadOnlySpan<byte> source, out bool result)
    {
        result = source[0] != 0;
        return source[sizeof(byte)..];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<byte> Read(this ReadOnlySpan<byte> source, out ushort result)
    {
        result = BinaryPrimitives.ReadUInt16BigEndian(source);
        return source[sizeof(ushort)..];
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<byte> Read(this ReadOnlySpan<byte> source, out short result)
    {
        result = BinaryPrimitives.ReadInt16BigEndian(source);
        return source[sizeof(short)..];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<byte> Read(this ReadOnlySpan<byte> source, out uint result)
    {
        result = BinaryPrimitives.ReadUInt32BigEndian(source);
        return source[sizeof(uint)..];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<byte> Read(this ReadOnlySpan<byte> source, out int result)
    {
        result = BinaryPrimitives.ReadInt32BigEndian(source);
        return source[sizeof(int)..];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<byte> Read(this ReadOnlySpan<byte> source, out ulong result)
    {
        result = BinaryPrimitives.ReadUInt64BigEndian(source);
        return source[sizeof(ulong)..];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<byte> Read(this ReadOnlySpan<byte> source, out long result)
    {
        result = BinaryPrimitives.ReadInt64BigEndian(source);
        return source[sizeof(long)..];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<byte> Read(this ReadOnlySpan<byte> source, out float result)
    {
        result = MemoryMarshal.Read<float>(source);
        return source[sizeof(float)..];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<byte> Read(this ReadOnlySpan<byte> source, out decimal result)
    {
        result = MemoryMarshal.Read<decimal>(source);
        return source[sizeof(decimal)..];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<byte> Read(this ReadOnlySpan<byte> source, out double result)
    {
        result = MemoryMarshal.Read<double>(source);
        return source[sizeof(double)..];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<byte> Read(this ReadOnlySpan<byte> source, out string result, bool isLongString = false)
    {
        int len;
        if (isLongString)
        {
            source = source.Read(out uint size);
            len = (int)size;
        }
        else
        {
            source = source.Read(out byte size);
            len = size;
        }

        if (len == 0)
        {
            result = string.Empty;
        }
        else
        {
            var data = source[..len];
            result = TextEncoding.GetString(data);
        }

        return source[len..];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<byte> Read(this ReadOnlySpan<byte> source, out DateTimeOffset result)
    {
        source = source.Read(out long timestamp);
        result = DateTimeOffset.FromUnixTimeSeconds(timestamp);
        return source;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<byte> Read(this ReadOnlySpan<byte> source, out byte[] result)
    {
        source = source.Read(out uint size);
        result = source[..(int)size].ToArray();
        return source[(int)size..];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<byte> ReadField(this ReadOnlySpan<byte> source, out object result)
    {
        source = source.Read(out byte fieldType);

        switch (fieldType)
        {
            case FieldTypePrefix.Boolean:
                source = source.Read(out bool boolData);
                result = boolData;
                break;
            case FieldTypePrefix.Byte:
                source = source.Read(out byte byteData);
                result = byteData;
                break;
            case FieldTypePrefix.SByte:
                source = source.Read(out sbyte sbyteData);
                result = sbyteData;
                break;
            case FieldTypePrefix.Short:
                source = source.Read(out short shortData);
                result = shortData;
                break;
            case FieldTypePrefix.UInt:
                source = source.Read(out uint uintData);
                result = uintData;
                break;
            case FieldTypePrefix.Int:
                source = source.Read(out int intData);
                result = intData;
                break;
            case FieldTypePrefix.Long:
                source = source.Read(out long longData);
                result = longData;
                break;
            case FieldTypePrefix.Single:
                source = source.Read(out float floatData);
                result = floatData;
                break;
            case FieldTypePrefix.Decimal:
                source = source.Read(out decimal decimalData);
                result = decimalData;
                break;
            case FieldTypePrefix.Double:
                source = source.Read(out double doubleData);
                result = doubleData;
                break;
            case FieldTypePrefix.Timestamp:
                source = source.Read(out DateTimeOffset timestampData);
                result = timestampData;
                break;
            case FieldTypePrefix.String:
                source = source.Read(out string stringData, true);
                result = stringData;
                break;
            case FieldTypePrefix.Array:
                source = source.Read(out object[] arrayData);
                result = arrayData;
                break;
            case FieldTypePrefix.Table:
                source = source.Read(out Dictionary<string, object> tableData);
                result = tableData;
                break;
            case FieldTypePrefix.Binary:
                source = source.Read(out byte[] binaryData);
                result = binaryData;
                break;
            case FieldTypePrefix.Null:
                result = null;
                break;
            default:
                throw new NotSupportedException($"Not supported type prefix: {(char)fieldType}");
        }

        return source;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<byte> Read(this ReadOnlySpan<byte> source, out Dictionary<string, object> result)
    {
        source = source.Read(out uint tableLen);
        if (tableLen == 0)
        {
            result = null;
            return source;
        }

        var tableData = source[..(int)tableLen];

        var data = new Dictionary<string, object>();
        while (tableData.Length > 0)
        {
            tableData = tableData.Read(out string fieldName);
            tableData = tableData.ReadField(out object value);
            data[fieldName] = value;
        }

        result = data;
        return source[(int)tableLen..];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<byte> Read(this ReadOnlySpan<byte> source, out object[] result)
    {
        source = source.Read(out uint size);
        if (size == 0)
        {
            result = null;
            return source;
        }

        var arrayData = source[..(int)size];

        var list = new List<object>();

        while (arrayData.Length > 0)
        {
            arrayData = arrayData.ReadField(out var item);
            list.Add(item);
        }

        result = list.ToArray();
        return source[(int)size..];
    }
}