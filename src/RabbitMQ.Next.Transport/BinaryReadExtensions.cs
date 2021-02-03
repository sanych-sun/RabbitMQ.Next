using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace RabbitMQ.Next.Transport
{
    public static class BinaryReadExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> Read(this ReadOnlySpan<byte> source, out byte result)
        {
            var data = source.Slice(0, sizeof(byte));
            result = data[0];
            return source.Slice(sizeof(byte));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> Read(this ReadOnlySpan<byte> source, out sbyte result)
        {
            var data = source.Slice(0, sizeof(sbyte));
            result = (sbyte)data[0];
            return source.Slice(sizeof(sbyte));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> Read(this ReadOnlySpan<byte> source, out bool result)
        {
            var data = source.Slice(0, sizeof(byte));
            result = (data[0] != 0);
            return source.Slice(sizeof(byte));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> Read(this ReadOnlySpan<byte> source, out ushort result)
        {
            var data = source.Slice(0, sizeof(ushort));
            result = BinaryPrimitives.ReadUInt16BigEndian(data);
            return source.Slice(sizeof(ushort));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> Read(this ReadOnlySpan<byte> source, out short result)
        {
            var data = source.Slice(0, sizeof(short));
            result = BinaryPrimitives.ReadInt16BigEndian(data);
            return source.Slice(sizeof(short));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> Read(this ReadOnlySpan<byte> source, out uint result)
        {
            var data = source.Slice(0, sizeof(uint));
            result = BinaryPrimitives.ReadUInt32BigEndian(data);
            return source.Slice(sizeof(uint));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> Read(this ReadOnlySpan<byte> source, out int result)
        {
            var data = source.Slice(0, sizeof(int));
            result = BinaryPrimitives.ReadInt32BigEndian(data);
            return source.Slice(sizeof(int));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> Read(this ReadOnlySpan<byte> source, out ulong result)
        {
            var data = source.Slice(0, sizeof(ulong));
            result = BinaryPrimitives.ReadUInt64BigEndian(data);
            return source.Slice(sizeof(ulong));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> Read(this ReadOnlySpan<byte> source, out long result)
        {
            var data = source.Slice(0, sizeof(long));
            result = BinaryPrimitives.ReadInt64BigEndian(data);
            return source.Slice(sizeof(long));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> Read(this ReadOnlySpan<byte> source, out float result)
        {
            var data = source.Slice(0, sizeof(float));
            result = MemoryMarshal.Read<float>(data);
            return source.Slice(sizeof(float));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> Read(this ReadOnlySpan<byte> source, out decimal result)
        {
            var data = source.Slice(0, sizeof(decimal));
            result = MemoryMarshal.Read<decimal>(data);
            return source.Slice(sizeof(decimal));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> Read(this ReadOnlySpan<byte> source, out double result)
        {
            var data = source.Slice(0, sizeof(double));
            result = MemoryMarshal.Read<double>(data);
            return source.Slice(sizeof(double));
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
                var data = source.Slice(0, len);
                result = TextEncoding.GetString(data);
            }

            return source.Slice(len);
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
            result = source.Slice(0, (int)size).ToArray();
            return source.Slice((int)size);
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
                    source = source.Read(out IReadOnlyDictionary<string, object> tableData);
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
        public static ReadOnlySpan<byte> Read(this ReadOnlySpan<byte> source, out IReadOnlyDictionary<string, object> result)
        {
            source = source.Read(out uint tableLen);
            if (tableLen == 0)
            {
                result = null;
                return source;
            }

            var tableData = source.Slice(0, (int)tableLen);

            var data = new Dictionary<string, object>();
            while (tableData.Length > 0)
            {
                tableData = tableData.Read(out string fieldName);
                tableData = tableData.ReadField(out object value);
                data[fieldName] = value;
            }

            result = data;
            return source.Slice((int)tableLen);
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

            var arrayData = source.Slice(0, (int)size);

            var list = new List<object>();

            while (arrayData.Length > 0)
            {
                arrayData = arrayData.ReadField(out var item);
                list.Add(item);
            }

            result = list.ToArray();
            return source.Slice((int)size);
        }
    }
}