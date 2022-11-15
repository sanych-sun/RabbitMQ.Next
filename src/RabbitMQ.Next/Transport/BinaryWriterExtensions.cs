using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace RabbitMQ.Next.Transport;

internal static class BinaryWriterExtensions
{
    public static IBinaryWriter Allocate(this IBinaryWriter writer, int bytes, out Memory<byte> buffer)
    {
        buffer = writer.GetMemory(bytes)[..bytes];
        buffer.Span.Fill(0);
        writer.Advance(bytes);
        return writer;
    }
    
    public static IBinaryWriter Write(this IBinaryWriter writer, byte data)
    {
        var buffer = writer.GetSpan(sizeof(byte));
        buffer[0] = data;
        writer.Advance(sizeof(byte));
        return writer;
    }

    
    public static IBinaryWriter Write(this IBinaryWriter writer, sbyte data)
    {
        var buffer = writer.GetSpan(sizeof(sbyte));
        buffer[0] = (byte)data;
        writer.Advance(sizeof(sbyte));
        return writer;
    }

    
    public static IBinaryWriter Write(this IBinaryWriter writer, bool data)
    {
        return writer.Write(data ? (byte) 1 : (byte) 0);
    }

    
    public static IBinaryWriter Write(this IBinaryWriter writer, ushort data)
    {
        var buffer = writer.GetSpan(sizeof(ushort));
        BinaryPrimitives.WriteUInt16BigEndian(buffer, data);
        writer.Advance(sizeof(ushort));
        return writer;
    }

    
    public static IBinaryWriter Write(this IBinaryWriter writer, short data)
    {
        var buffer = writer.GetSpan(sizeof(short));
        BinaryPrimitives.WriteInt16BigEndian(buffer, data);
        writer.Advance(sizeof(short));
        return writer;
    }

    
    public static IBinaryWriter Write(this IBinaryWriter writer, uint data)
    {
        var buffer = writer.GetSpan(sizeof(uint));
        BinaryPrimitives.WriteUInt32BigEndian(buffer, data);
        writer.Advance(sizeof(uint));
        return writer;
    }

    
    public static IBinaryWriter Write(this IBinaryWriter writer, int data)
    {
        var buffer = writer.GetSpan(sizeof(int));
        BinaryPrimitives.WriteInt32BigEndian(buffer, data);
        writer.Advance(sizeof(int));
        return writer;
    }

    
    public static IBinaryWriter Write(this IBinaryWriter writer, ulong data)
    {
        var buffer = writer.GetSpan(sizeof(ulong));
        BinaryPrimitives.WriteUInt64BigEndian(buffer, data);
        writer.Advance(sizeof(ulong));
        return writer;
    }

    
    public static IBinaryWriter Write(this IBinaryWriter writer, long data)
    {
        var buffer = writer.GetSpan(sizeof(long));
        BinaryPrimitives.WriteInt64BigEndian(buffer, data);
        writer.Advance(sizeof(long));
        return writer;
    }

    
    public static IBinaryWriter Write(this IBinaryWriter writer, float data)
    {
        var buffer = writer.GetSpan(sizeof(float));
        MemoryMarshal.Write(buffer, ref data);
        writer.Advance(sizeof(float));
        return writer;
    }

    
    public static IBinaryWriter Write(this IBinaryWriter writer, decimal data)
    {
        var buffer = writer.GetSpan(sizeof(decimal));
        MemoryMarshal.Write(buffer, ref data);
        writer.Advance(sizeof(decimal));
        return writer;
    }

    
    public static IBinaryWriter Write(this IBinaryWriter writer, double data)
    {
        var buffer = writer.GetSpan(sizeof(double));
        MemoryMarshal.Write(buffer, ref data);
        writer.Advance(sizeof(double));
        return writer;
    }

    
    public static IBinaryWriter Write(this IBinaryWriter writer, ReadOnlySpan<byte> data)
    {
        writer.Write((uint)data.Length);
        var buffer = writer.GetSpan(data.Length);
        data.CopyTo(buffer);
        writer.Advance(data.Length);
        return writer;
    }
    
    public static IBinaryWriter WriteFlags(
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

    
    public static IBinaryWriter Write(this IBinaryWriter writer, string data, bool isLongString = false)
    {
        var sizeBufferLen = isLongString ? sizeof(uint) : sizeof(byte);
        writer.Allocate(sizeBufferLen, out var sizeBuffer);

        if (string.IsNullOrEmpty(data))
        {
            return writer;
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
        return writer;
    }

    
    public static IBinaryWriter Write(this IBinaryWriter writer, DateTimeOffset data)
    {
        var timestamp = data.ToUnixTimeSeconds();
        return writer.Write(timestamp);
    }

    
    internal static void WriteField(this IBinaryWriter writer, object value)
    {
        switch (value)
        {
            case bool boolValue:
                writer.Write(FieldTypePrefix.Boolean);
                writer.Write(boolValue);
                break;
            case byte byteValue:
                writer.Write(FieldTypePrefix.Byte);
                writer.Write(byteValue);
                break;
            case sbyte sbyteValue:
                writer.Write(FieldTypePrefix.SByte);
                writer.Write(sbyteValue);
                break;
            case short shortValue:
                writer.Write(FieldTypePrefix.Short);
                writer.Write(shortValue);
                break;
            case uint uintValue:
                writer.Write(FieldTypePrefix.UInt);
                writer.Write(uintValue);
                break;
            case int intValue:
                writer.Write(FieldTypePrefix.Int);
                writer.Write(intValue);
                break;
            case long longValue:
                writer.Write(FieldTypePrefix.Long);
                writer.Write(longValue);
                break;
            case float floatValue:
                writer.Write(FieldTypePrefix.Single);
                writer.Write(floatValue);
                break;
            case decimal decimalValue:
                writer.Write(FieldTypePrefix.Decimal);
                writer.Write(decimalValue);
                break;
            case double doubleValue:
                writer.Write(FieldTypePrefix.Double);
                writer.Write(doubleValue);
                break;
            case DateTimeOffset dateValue:
                writer.Write(FieldTypePrefix.Timestamp);
                writer.Write(dateValue);
                break;
            case string stringValue:
                writer.Write(FieldTypePrefix.String);
                writer.Write(stringValue, true);
                break;
            case object[] arrayValue:
                writer.Write(FieldTypePrefix.Array);
                writer.Write(arrayValue);
                break;
            case IReadOnlyDictionary<string, object> tableValue:
                writer.Write(FieldTypePrefix.Table);
                writer.Write(tableValue);
                break;
            case byte[] binaryValue:
                writer.Write(FieldTypePrefix.Binary);
                writer.Write(binaryValue);
                break;
            case null:
                writer.Write(FieldTypePrefix.Null);
                break;
            default:
                throw new NotSupportedException($"Not supported type: {value.GetType().FullName}");
        }
    }

    
    public static IBinaryWriter Write(this IBinaryWriter writer, IEnumerable<KeyValuePair<string, object>> value)
    {
        if (value == null)
        {
            writer.Write((uint)0);
            return writer;
        }

        writer.Allocate(sizeof(uint), out var sizeBuffer);

        var initialSize = writer.BytesWritten;
        foreach (var (key, o) in value)
        {
            writer
                .Write(key)
                .WriteField(o);
        }

        BinaryPrimitives.WriteUInt32BigEndian(sizeBuffer.Span, (uint) (writer.BytesWritten - initialSize));
        return writer;
    }

    
    public static IBinaryWriter Write(this IBinaryWriter writer, object[] value)
    {
        if (value == null)
        {
            writer.Write((uint)0);
            return writer;
        }

        var sizeBuffer = writer.GetSpan(sizeof(uint));
        writer.Advance(sizeof(uint));

        var initialSize = writer.BytesWritten;
        
        for (var i = 0; i < value.Length; i++)
        {
            writer.WriteField(value[i]);
        }

        BinaryPrimitives.WriteUInt32BigEndian(sizeBuffer, (uint) (writer.BytesWritten - initialSize));
        return writer;
    }
}