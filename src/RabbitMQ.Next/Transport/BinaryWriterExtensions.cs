using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RabbitMQ.Next.Transport;

internal static class BinaryWriterExtensions
{
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Allocate(this IBinaryWriter writer, int bytes, out Memory<byte> buffer)
    {
        buffer = writer.GetMemory(bytes)[..bytes];
        buffer.Span.Fill(0);
        writer.Advance(bytes);
        return bytes;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Write(this IBinaryWriter writer, byte data)
    {
        var buffer = writer.GetSpan(sizeof(byte));
        buffer[0] = data;
        writer.Advance(sizeof(byte));
        return sizeof(byte);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Write(this IBinaryWriter writer, sbyte data)
    {
        var buffer = writer.GetSpan(sizeof(sbyte));
        buffer[0] = (byte)data;
        writer.Advance(sizeof(sbyte));
        return sizeof(sbyte);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Write(this IBinaryWriter writer, bool data)
    {
        return writer.Write(data ? (byte) 1 : (byte) 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Write(this IBinaryWriter writer, ushort data)
    {
        var buffer = writer.GetSpan(sizeof(ushort));
        BinaryPrimitives.WriteUInt16BigEndian(buffer, data);
        writer.Advance(sizeof(ushort));
        return sizeof(ushort);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Write(this IBinaryWriter writer, short data)
    {
        var buffer = writer.GetSpan(sizeof(short));
        BinaryPrimitives.WriteInt16BigEndian(buffer, data);
        writer.Advance(sizeof(short));
        return sizeof(short);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Write(this IBinaryWriter writer, uint data)
    {
        var buffer = writer.GetSpan(sizeof(uint));
        BinaryPrimitives.WriteUInt32BigEndian(buffer, data);
        writer.Advance(sizeof(uint));
        return sizeof(uint);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Write(this IBinaryWriter writer, int data)
    {
        var buffer = writer.GetSpan(sizeof(int));
        BinaryPrimitives.WriteInt32BigEndian(buffer, data);
        writer.Advance(sizeof(int));
        return sizeof(int);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Write(this IBinaryWriter writer, ulong data)
    {
        var buffer = writer.GetSpan(sizeof(ulong));
        BinaryPrimitives.WriteUInt64BigEndian(buffer, data);
        writer.Advance(sizeof(ulong));
        return sizeof(ulong);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Write(this IBinaryWriter writer, long data)
    {
        var buffer = writer.GetSpan(sizeof(long));
        BinaryPrimitives.WriteInt64BigEndian(buffer, data);
        writer.Advance(sizeof(long));
        return sizeof(long);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Write(this IBinaryWriter writer, float data)
    {
        var buffer = writer.GetSpan(sizeof(float));
        MemoryMarshal.Write(buffer, ref data);
        writer.Advance(sizeof(float));
        return sizeof(float);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Write(this IBinaryWriter writer, decimal data)
    {
        var buffer = writer.GetSpan(sizeof(decimal));
        MemoryMarshal.Write(buffer, ref data);
        writer.Advance(sizeof(decimal));
        return sizeof(decimal);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Write(this IBinaryWriter writer, double data)
    {
        var buffer = writer.GetSpan(sizeof(double));
        MemoryMarshal.Write(buffer, ref data);
        writer.Advance(sizeof(double));
        return sizeof(double);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Write(this IBinaryWriter writer, ReadOnlySpan<byte> data)
    {
        var written = writer.Write((uint)data.Length);
        var buffer = writer.GetSpan(data.Length);
        data.CopyTo(buffer);
        writer.Advance(data.Length);
        written += data.Length;
        return written;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int WriteFlags(
        this IBinaryWriter writer,
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

        return writer.Write(bits);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Write(this IBinaryWriter writer, string data, bool isLongString = false)
    {
        var sizeBufferLen = isLongString ? sizeof(uint) : sizeof(byte);
        writer.Allocate(sizeBufferLen, out var sizeBuffer);

        if (string.IsNullOrEmpty(data))
        {
            return sizeBufferLen;
        }
        
        var buffer = writer.GetSpan(TextEncoding.GetMaxBytes(data.Length));
        var bytesWritten = TextEncoding.GetBytes(data, buffer);
        
        if (isLongString)
        {
            BinaryPrimitives.WriteUInt32BigEndian(sizeBuffer.Span, (uint) bytesWritten);
        }
        else
        {
            if (bytesWritten > 255)
            {
                throw new ArgumentException("Short string should be less then 256 bytes", nameof(data));
            }

            sizeBuffer.Span[0] = (byte)bytesWritten;
        }
        
        writer.Advance(bytesWritten);
        return sizeBufferLen + bytesWritten;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Write(this IBinaryWriter writer, DateTimeOffset data)
    {
        var timestamp = data.ToUnixTimeSeconds();
        return writer.Write(timestamp);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int WriteField(this IBinaryWriter writer, object value)
    {
        var written = 0;
        switch (value)
        {
            case bool boolValue:
                written = writer.Write(FieldTypePrefix.Boolean);
                written += writer.Write(boolValue);
                break;
            case byte byteValue:
                written = writer.Write(FieldTypePrefix.Byte);
                written += writer.Write(byteValue);
                break;
            case sbyte sbyteValue:
                written = writer.Write(FieldTypePrefix.SByte);
                written +=  writer.Write(sbyteValue);
                break;
            case short shortValue:
                written = writer.Write(FieldTypePrefix.Short);
                written +=  writer.Write(shortValue);
                break;
            case uint uintValue:
                written = writer.Write(FieldTypePrefix.UInt);
                written += writer.Write(uintValue);
                break;
            case int intValue:
                written = writer.Write(FieldTypePrefix.Int);
                written += writer.Write(intValue);
                break;
            case long longValue:
                written = writer.Write(FieldTypePrefix.Long);
                written += writer.Write(longValue);
                break;
            case float floatValue:
                written = writer.Write(FieldTypePrefix.Single);
                written += writer.Write(floatValue);
                break;
            case decimal decimalValue:
                written = writer.Write(FieldTypePrefix.Decimal);
                written += writer.Write(decimalValue);
                break;
            case double doubleValue:
                written = writer.Write(FieldTypePrefix.Double);
                written += writer.Write(doubleValue);
                break;
            case DateTimeOffset dateValue:
                written = writer.Write(FieldTypePrefix.Timestamp);
                written += writer.Write(dateValue);
                break;
            case string stringValue:
                written = writer.Write(FieldTypePrefix.String);
                written += writer.Write(stringValue, true);
                break;
            case object[] arrayValue:
                written = writer.Write(FieldTypePrefix.Array);
                written += writer.Write(arrayValue);
                break;
            case IReadOnlyDictionary<string, object> tableValue:
                written = writer.Write(FieldTypePrefix.Table);
                written += writer.Write(tableValue);
                break;
            case byte[] binaryValue:
                written = writer.Write(FieldTypePrefix.Binary);
                written += writer.Write(binaryValue);
                break;
            case null:
                written = writer.Write(FieldTypePrefix.Null);
                break;
            default:
                throw new NotSupportedException($"Not supported type: {value.GetType().FullName}");
        }

        return written;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Write(this IBinaryWriter writer, IEnumerable<KeyValuePair<string, object>> value)
    {
        if (value == null)
        {
            return writer.Write((uint)0);
        }

        var written = writer.Allocate(sizeof(uint), out var sizeBuffer);

        var dataBytes = 0;
        foreach (var (key, o) in value)
        {
            dataBytes += writer.Write(key);
            dataBytes += writer.WriteField(o);
        }

        BinaryPrimitives.WriteUInt32BigEndian(sizeBuffer.Span, (uint) dataBytes);
        written += dataBytes;
        return written;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Write(this IBinaryWriter writer, object[] value)
    {
        if (value == null)
        {
            return writer.Write((uint)0);
        }

        var written = writer.Allocate(sizeof(uint), out var sizeBuffer);

        var dataBytes = 0;
        for (var i = 0; i < value.Length; i++)
        {
            dataBytes += writer.WriteField(value[i]);
        }

        BinaryPrimitives.WriteUInt32BigEndian(sizeBuffer.Span, (uint) dataBytes);
        written += dataBytes;
        return written;
    }
}