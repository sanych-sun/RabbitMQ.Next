using System;
using System.Collections.Generic;
using RabbitMQ.Next.Transport;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport
{
    public class BinaryWriteExtensionsTests
    {
        [Theory]
        [InlineData(0, new byte[] { 0 })]
        [InlineData(1, new byte[] { 1 })]
        [InlineData(214, new byte[] { 214 })]
        public void WriteByte(byte data, byte[] expected)
        {
            Span<byte> buffer = stackalloc byte[expected.Length];
            buffer.Write(data);

            Assert.Equal(expected, buffer.ToArray());
        }

        [Theory]
        [InlineData(0, new byte[] { 0 })]
        [InlineData(1, new byte[] { 1 })]
        [InlineData(-42, new byte[] { 214 })]
        public void WriteSByte(sbyte data, byte[] expected)
        {
            Span<byte> buffer = stackalloc byte[expected.Length];
            buffer.Write(data);

            Assert.Equal(expected, buffer.ToArray());
        }

        [Theory]
        [InlineData(false, new byte[] { 0 })]
        [InlineData(true, new byte[] { 1 })]
        public void WriteBool(bool data, byte[] expected)
        {
            Span<byte> buffer = stackalloc byte[expected.Length];
            buffer.Write(data);

            Assert.Equal(expected, buffer.ToArray());
        }

        [Theory]
        [InlineData(0, new byte[] { 0, 0 })]
        [InlineData(256, new byte[] { 1, 0 })]
        [InlineData(54786, new byte[] { 214, 2 })]
        public void WriteUShort(ushort data, byte[] expected)
        {
            Span<byte> buffer = stackalloc byte[expected.Length];
            buffer.Write(data);

            Assert.Equal(expected, buffer.ToArray());
        }

        [Theory]
        [InlineData(0, new byte[] { 0, 0 })]
        [InlineData(256, new byte[] { 1, 0 })]
        [InlineData(-10750, new byte[] { 214, 2 })]
        public void WriteShort(short data, byte[] expected)
        {
            Span<byte> buffer = stackalloc byte[expected.Length];
            buffer.Write(data);

            Assert.Equal(expected, buffer.ToArray());
        }

        [Theory]
        [InlineData(0, new byte[] { 0, 0, 0, 0 })]
        [InlineData(16777216, new byte[] { 1, 0, 0, 0 })]
        [InlineData(3590324224, new byte[] { 214, 0, 0, 0 })]
        public void WriteUInt(uint data, byte[] expected)
        {
            Span<byte> buffer = stackalloc byte[expected.Length];
            buffer.Write(data);

            Assert.Equal(expected, buffer.ToArray());
        }

        [Theory]
        [InlineData(0, new byte[] { 0, 0, 0, 0 })]
        [InlineData(16777216, new byte[] { 1, 0, 0, 0 })]
        [InlineData(-704643072, new byte[] { 214, 0, 0, 0 })]
        public void WriteInt(int data, byte[] expected)
        {
            Span<byte> buffer = stackalloc byte[expected.Length];
            buffer.Write(data);

            Assert.Equal(expected, buffer.ToArray());
        }

        [Theory]
        [InlineData(0, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 })]
        [InlineData(72057594037927936, new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 })]
        [InlineData(15420325124116578304, new byte[] { 214, 0, 0, 0, 0, 0, 0, 0 })]
        public void WriteULong(ulong data, byte[] expected)
        {
            Span<byte> buffer = stackalloc byte[expected.Length];
            buffer.Write(data);
            
            Assert.Equal(expected, buffer.ToArray());
        }

        [Theory]
        [InlineData(0, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 })]
        [InlineData(72057594037927936, new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 })]
        [InlineData(-3026418949592973312, new byte[] { 214, 0, 0, 0, 0, 0, 0, 0 })]
        public void WriteLong(long data, byte[] expected)
        {
            Span<byte> buffer = stackalloc byte[expected.Length];
            buffer.Write(data);

            Assert.Equal(expected, buffer.ToArray());
        }

        [Theory]
        [InlineData(0, new byte[] { 0, 0, 0, 0 })]
        [InlineData(1E-45, new byte[] { 1, 0, 0, 0 })]
        public void WriteFloat(float data, byte[] expected)
        {
            Span<byte> buffer = stackalloc byte[expected.Length];
            buffer.Write(data);

            Assert.Equal(expected, buffer.ToArray());
        }

        [Theory]
        [InlineData(0, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 })]
        [InlineData(5E-324, new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 })]
        public void WriteDouble(double data, byte[] expected)
        {
            Span<byte> buffer = stackalloc byte[expected.Length];
            buffer.Write(data);

            Assert.Equal(expected, buffer.ToArray());
        }

        [Theory]
        [InlineData(0d, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 })]
        [InlineData(0.32d, new byte[] { 0, 0, 2, 0, 0, 0, 0, 0, 32, 0, 0, 0, 0, 0, 0, 0 })]
        public void WriteDecimal(decimal data, byte[] expected)
        {
            Span<byte> buffer = stackalloc byte[expected.Length];
            buffer.Write(data);

            Assert.Equal(expected, buffer.ToArray());
        }

        [Theory]
        [InlineData("", false, new byte[] { 0 })]
        [InlineData("Hello", false, new byte[] { 5, 72, 101, 108, 108, 111 })]
        [InlineData("", true, new byte[] { 0, 0, 0, 0 })]
        [InlineData("Hello", true, new byte[] { 0, 0, 0, 5, 72, 101, 108, 108, 111 })]
        public void WriteString(string data, bool isLong, byte[] expected)
        {
            Span<byte> buffer = stackalloc byte[expected.Length];
            buffer.Write(data, isLong);

            Assert.Equal(expected, buffer.ToArray());
        }

        [Theory]
        [InlineData(0, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 })]
        [InlineData(433555200, new byte[] { 0, 0, 0, 0, 25, 215, 135, 0 })]
        public void WriteDateTime(long data, byte[] expected)
        {
            var dt = DateTimeOffset.FromUnixTimeSeconds(data);
            Span<byte> buffer = stackalloc byte[expected.Length];
            buffer.Write(dt);

            Assert.Equal(expected, buffer.ToArray());
        }

        [Theory]
        [InlineData(new byte[0], new byte[] { 0, 0, 0, 0 })]
        [InlineData(new byte[] { 1 },new byte[] { 0, 0, 0, 1, 1 })]
        [InlineData(new byte[] { 1, 2, 3, 4, 5 }, new byte[] { 0, 0, 0, 5, 1, 2, 3, 4, 5 })]
        public void WriteBinary(byte[] data, byte[] expected)
        {
            Span<byte> buffer = stackalloc byte[expected.Length];
            buffer.Write(data);

            Assert.Equal(expected, buffer.ToArray());
        }

        [Theory]
        [MemberData(nameof(WriteFieldTestCases))]
        public void WriteField(object value, byte[] expected)
        {
            Span<byte> buffer = stackalloc byte[expected.Length];
            buffer.WriteField(value);

            Assert.Equal(expected, buffer.ToArray());
        }

        [Theory]
        [MemberData(nameof(WriteDictionaryTestCases))]
        public void WriteDictionary(IReadOnlyDictionary<string, object> value, byte[] expected)
        {
            Span<byte> buffer = stackalloc byte[expected.Length];
            buffer.Write(value);

            Assert.Equal(expected, buffer.ToArray());
        }

        [Theory]
        [MemberData(nameof(WriteArrayTestCases))]
        public void WriteArray(object[] value, byte[] expected)
        {
            Span<byte> buffer = stackalloc byte[expected.Length];
            buffer.Write(value);

            Assert.Equal(expected, buffer.ToArray());
        }

        [Theory]
        [InlineData(new byte[] { 0b_00000000}, false)]
        [InlineData(new byte[] { 0b_00000001}, true)]
        [InlineData(new byte[] { 0b_00000010}, false, true)]
        [InlineData(new byte[] { 0b_00000100}, false, false, true)]
        [InlineData(new byte[] { 0b_00001000}, false, false, false, true)]
        [InlineData(new byte[] { 0b_00010000}, false, false, false, false, true)]
        [InlineData(new byte[] { 0b_00100000}, false, false, false, false, false, true)]
        [InlineData(new byte[] { 0b_01000000}, false, false, false, false, false, false, true)]
        [InlineData(new byte[] { 0b_10000000}, false, false, false, false, false, false, false, true)]
        [InlineData(new byte[] { 0b_11111111}, true, true, true, true, true, true, true, true)]
        public void WriteBits(
            byte[] expected,
            bool bit1, bool bit2 = false, bool bit3 = false, bool bit4 = false,
            bool bit5 = false, bool bit6 = false, bool bit7 = false, bool bit8 = false)
        {
            Span<byte> buffer = stackalloc byte[expected.Length];

            buffer.WriteBits(bit1, bit2, bit3, bit4, bit5, bit6, bit7, bit8);

            Assert.Equal(expected, buffer.ToArray());
        }

        [Theory]
        [InlineData(256)]
        public void WriteThrowOnTooLongString(int length)
        {
            var data = Helpers.GetDummyText(length);

            Assert.Throws<ArgumentException>(() =>
            {
                Span<byte> buffer = stackalloc byte[1000];
                buffer.Write(data, false);
            });
        }

        [Theory]
        [InlineData((ulong)42)]
        [InlineData((ushort)42)]
        public void WriteFieldThrowOnNonSupportedFieldType(object value)
        {
            Assert.Throws<NotSupportedException>(() =>
            {
                Span<byte> buffer = stackalloc byte[1000];
                buffer.WriteField(value);
            });
        }

        public static IEnumerable<object[]> WriteFieldTestCases()
        {
            yield return new object[] { true, new byte[] {116, 1}};
            yield return new object[] { (byte)42, new byte[] { 66, 42 }};
            yield return new object[] { (sbyte)42, new byte[] { 98, 42 }};
            yield return new object[] { (short)42, new byte[] { 115, 0, 42 }};
            yield return new object[] { (uint)42, new byte[] { 105, 0, 0, 0, 42 }};
            yield return new object[] { (int)42, new byte[] { 73, 0, 0, 0, 42 }};
            yield return new object[] { (long)42, new byte[] { 108, 0, 0, 0, 0, 0, 0, 0, 42 }};
            yield return new object[] { (float)3.14, new byte[] { 102, 195, 245, 72, 64 }};
            yield return new object[] { (double)3.14, new byte[] { 100, 31, 133, 235, 81, 184, 30, 9, 64 }};
            yield return new object[] { (decimal)3.14, new byte[] { 68, 0, 0, 2, 0, 0, 0, 0, 0, 58, 1, 0, 0, 0, 0, 0, 0 }};
            yield return new object[] { new DateTimeOffset(1983, 09,28, 0, 0, 0, TimeSpan.Zero), new byte[] { 84, 0, 0, 0, 0, 25, 215, 135, 0 }};
            yield return new object[] { "Hello", new byte[] { 83, 0, 0, 0, 5, 72, 101, 108, 108, 111 } };
            yield return new object[] { new object[] {(byte)42, (float)3.14, "Hello"}, new byte[] { 65, 0, 0, 0, 17, 66, 42, 102, 195, 245, 72, 64, 83, 0, 0, 0, 5, 72, 101, 108, 108, 111 }};
            yield return new object[] { new Dictionary<string, object>() { ["key"] = "value" }, new byte[] { 70, 0, 0, 0, 14, 3, 107, 101, 121, 83, 0, 0, 0, 5, 118, 97, 108, 117, 101 } };
            yield return new object[] { new byte[] { 0, 1, 2, 3, 4}, new byte[] { 120, 0, 0, 0, 5, 0, 1, 2, 3, 4 } };
            yield return new object[] { null, new byte[] { 86 }};
        }

        public static IEnumerable<object[]> WriteDictionaryTestCases()
        {
            yield return new object[] { null, new byte[] { 0, 0, 0, 0 }};
            yield return new object[] { new Dictionary<string, object>(), new byte[] { 0, 0, 0, 0 }};
            yield return new object[] { new Dictionary<string, object>() { ["key"] = "value" }, new byte[] { 0, 0, 0, 14, 3, 107, 101, 121, 83, 0, 0, 0, 5, 118, 97, 108, 117, 101 }};
        }

        public static IEnumerable<object[]> WriteArrayTestCases()
        {
            yield return new object[] { null, new byte[] { 0, 0, 0, 0 }};
            yield return new object[] { new object[0], new byte[] { 0, 0, 0, 0 }};
            yield return new object[] { new object[] {(byte)42, (float)3.14, "Hello"}, new byte[] { 0, 0, 0, 17, 66, 42, 102, 195, 245, 72, 64, 83, 0, 0, 0, 5, 72, 101, 108, 108, 111 }};
        }
    }
}