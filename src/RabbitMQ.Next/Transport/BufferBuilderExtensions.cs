using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace RabbitMQ.Next.Transport;

internal static class BufferBuilderExtensions
{
    public static IBufferBuilder Allocate(this IBufferBuilder builder, int bytes, out Memory<byte> buffer)
    {
        buffer = builder.GetMemory(bytes)[..bytes];
        buffer.Span.Fill(0);
        builder.Advance(bytes);
        return builder;
    }
    
    public static IBufferBuilder Write(this IBufferBuilder builder, byte data)
    {
        var buffer = builder.GetSpan(sizeof(byte));
        buffer[0] = data;
        builder.Advance(sizeof(byte));
        return builder;
    }

    
    public static IBufferBuilder Write(this IBufferBuilder builder, sbyte data)
    {
        var buffer = builder.GetSpan(sizeof(sbyte));
        buffer[0] = (byte)data;
        builder.Advance(sizeof(sbyte));
        return builder;
    }

    
    public static IBufferBuilder Write(this IBufferBuilder builder, bool data)
    {
        return builder.Write(data ? (byte) 1 : (byte) 0);
    }

    
    public static IBufferBuilder Write(this IBufferBuilder builder, ushort data)
    {
        var buffer = builder.GetSpan(sizeof(ushort));
        BinaryPrimitives.WriteUInt16BigEndian(buffer, data);
        builder.Advance(sizeof(ushort));
        return builder;
    }

    
    public static IBufferBuilder Write(this IBufferBuilder builder, short data)
    {
        var buffer = builder.GetSpan(sizeof(short));
        BinaryPrimitives.WriteInt16BigEndian(buffer, data);
        builder.Advance(sizeof(short));
        return builder;
    }

    
    public static IBufferBuilder Write(this IBufferBuilder builder, uint data)
    {
        var buffer = builder.GetSpan(sizeof(uint));
        BinaryPrimitives.WriteUInt32BigEndian(buffer, data);
        builder.Advance(sizeof(uint));
        return builder;
    }

    
    public static IBufferBuilder Write(this IBufferBuilder builder, int data)
    {
        var buffer = builder.GetSpan(sizeof(int));
        BinaryPrimitives.WriteInt32BigEndian(buffer, data);
        builder.Advance(sizeof(int));
        return builder;
    }

    
    public static IBufferBuilder Write(this IBufferBuilder builder, ulong data)
    {
        var buffer = builder.GetSpan(sizeof(ulong));
        BinaryPrimitives.WriteUInt64BigEndian(buffer, data);
        builder.Advance(sizeof(ulong));
        return builder;
    }

    
    public static IBufferBuilder Write(this IBufferBuilder builder, long data)
    {
        var buffer = builder.GetSpan(sizeof(long));
        BinaryPrimitives.WriteInt64BigEndian(buffer, data);
        builder.Advance(sizeof(long));
        return builder;
    }

    
    public static IBufferBuilder Write(this IBufferBuilder builder, float data)
    {
        var buffer = builder.GetSpan(sizeof(float));
        MemoryMarshal.Write(buffer, ref data);
        builder.Advance(sizeof(float));
        return builder;
    }

    
    public static IBufferBuilder Write(this IBufferBuilder builder, decimal data)
    {
        var buffer = builder.GetSpan(sizeof(decimal));
        MemoryMarshal.Write(buffer, ref data);
        builder.Advance(sizeof(decimal));
        return builder;
    }

    
    public static IBufferBuilder Write(this IBufferBuilder builder, double data)
    {
        var buffer = builder.GetSpan(sizeof(double));
        MemoryMarshal.Write(buffer, ref data);
        builder.Advance(sizeof(double));
        return builder;
    }

    
    public static IBufferBuilder Write(this IBufferBuilder builder, ReadOnlySpan<byte> data)
    {
        builder.Write((uint)data.Length);
        var buffer = builder.GetSpan(data.Length);
        data.CopyTo(buffer);
        builder.Advance(data.Length);
        return builder;
    }

    
    public static IBufferBuilder Write(this IBufferBuilder builder, string data, bool isLongString = false)
    {
        var sizeBufferLen = isLongString ? sizeof(uint) : sizeof(byte);
        builder.Allocate(sizeBufferLen, out var sizeBuffer);

        if (string.IsNullOrEmpty(data))
        {
            return builder;
        }
        
        var buffer = builder.GetSpan(TextEncoding.GetMaxBytes(data.Length));
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
        
        builder.Advance(bytesWritten);
        return builder;
    }

    
    public static IBufferBuilder Write(this IBufferBuilder builder, DateTimeOffset data)
    {
        var timestamp = data.ToUnixTimeSeconds();
        return builder.Write(timestamp);
    }

    
    internal static void WriteField(this IBufferBuilder builder, object value)
    {
        switch (value)
        {
            case bool boolValue:
                builder.Write(FieldTypePrefix.Boolean);
                builder.Write(boolValue);
                break;
            case byte byteValue:
                builder.Write(FieldTypePrefix.Byte);
                builder.Write(byteValue);
                break;
            case sbyte sbyteValue:
                builder.Write(FieldTypePrefix.SByte);
                builder.Write(sbyteValue);
                break;
            case short shortValue:
                builder.Write(FieldTypePrefix.Short);
                builder.Write(shortValue);
                break;
            case uint uintValue:
                builder.Write(FieldTypePrefix.UInt);
                builder.Write(uintValue);
                break;
            case int intValue:
                builder.Write(FieldTypePrefix.Int);
                builder.Write(intValue);
                break;
            case long longValue:
                builder.Write(FieldTypePrefix.Long);
                builder.Write(longValue);
                break;
            case float floatValue:
                builder.Write(FieldTypePrefix.Single);
                builder.Write(floatValue);
                break;
            case decimal decimalValue:
                builder.Write(FieldTypePrefix.Decimal);
                builder.Write(decimalValue);
                break;
            case double doubleValue:
                builder.Write(FieldTypePrefix.Double);
                builder.Write(doubleValue);
                break;
            case DateTimeOffset dateValue:
                builder.Write(FieldTypePrefix.Timestamp);
                builder.Write(dateValue);
                break;
            case string stringValue:
                builder.Write(FieldTypePrefix.String);
                builder.Write(stringValue, true);
                break;
            case object[] arrayValue:
                builder.Write(FieldTypePrefix.Array);
                builder.Write(arrayValue);
                break;
            case IReadOnlyDictionary<string, object> tableValue:
                builder.Write(FieldTypePrefix.Table);
                builder.Write(tableValue);
                break;
            case byte[] binaryValue:
                builder.Write(FieldTypePrefix.Binary);
                builder.Write(binaryValue);
                break;
            case null:
                builder.Write(FieldTypePrefix.Null);
                break;
            default:
                throw new NotSupportedException($"Not supported type: {value.GetType().FullName}");
        }
    }

    
    public static IBufferBuilder Write(this IBufferBuilder builder, IEnumerable<KeyValuePair<string, object>> value)
    {
        if (value == null)
        {
            builder.Write((uint)0);
            return builder;
        }

        builder.Allocate(sizeof(uint), out var sizeBuffer);

        var initialSize = builder.BytesWritten;
        foreach (var (key, o) in value)
        {
            builder
                .Write(key)
                .WriteField(o);
        }

        BinaryPrimitives.WriteUInt32BigEndian(sizeBuffer.Span, (uint) (builder.BytesWritten - initialSize));
        return builder;
    }

    
    public static IBufferBuilder Write(this IBufferBuilder builder, object[] value)
    {
        if (value == null)
        {
            builder.Write((uint)0);
            return builder;
        }

        var sizeBuffer = builder.GetSpan(sizeof(uint));
        builder.Advance(sizeof(uint));

        var initialSize = builder.BytesWritten;
        
        for (var i = 0; i < value.Length; i++)
        {
            builder.WriteField(value[i]);
        }

        BinaryPrimitives.WriteUInt32BigEndian(sizeBuffer, (uint) (builder.BytesWritten - initialSize));
        return builder;
    }
}