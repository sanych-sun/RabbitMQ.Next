using System;
using System.Collections.Generic;
using RabbitMQ.Next.Tests.Mocks;
using RabbitMQ.Next.Transport;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport;

public class BinaryWriteExtensionsTests
{
    [Theory]
    [InlineData(new byte[] {0x01, 0x02}, 0, new byte[0], new byte[] {0x01, 0x02})]
    [InlineData(new byte[] {0x01, 0x02}, 1, new byte[] {0x01}, new byte[] {0x02})]
    [InlineData(new byte[] {0x01, 0x02, 0x03}, 3, new byte[] {0x01, 0x02, 0x03}, new byte[0])]
    [InlineData(new byte[] {0x01, 0x02, 0x03, 0x04, 0x05}, 3, new byte[] {0x01, 0x02, 0x03}, new byte[] {0x04, 0x05})]
    public void Slice(byte[] bytes, int size, byte[] slice, byte[] result)
    {
        var res = ((Span<byte>)bytes).Slice(size, out var resSlice);
        
        Assert.Equal(slice, resSlice.ToArray());
        Assert.Equal(result, res.ToArray());
    }

    [Fact]
    public void SliceThrowOnNegativeSize()
    {
        var buffer = new byte[10];

        Assert.Throws<ArgumentException>(() =>
        {
            ((Span<byte>)buffer).Slice(-5, out var _);    
        });
    }
    
    
    [Fact]
    public void SliceThrowOnTooBigSize()
    {
        var buffer = new byte[10];

        Assert.Throws<OutOfMemoryException>(() =>
        {
            ((Span<byte>)buffer).Slice(15, out var _);    
        });
    }
    
    [Theory]
    [InlineData(0, new byte[] { 0b_00000000 })]
    [InlineData(1, new byte[] { 0b_00000001 })]
    [InlineData(42, new byte[] { 0b_00101010 })]
    [InlineData(byte.MaxValue, new byte[] { 0b_11111111 })]
    public void WriteByte(byte data, byte[] expected)
    {
        Span<byte> buffer = stackalloc byte[expected.Length];
        buffer.Write(data);

        Assert.Equal(expected, buffer.ToArray());
    }

    [Theory]
    [InlineData(0, new byte[] { 0b_00000000 })]
    [InlineData(1, new byte[] { 0b_00000001 })]
    [InlineData(-1, new byte[] { 0b_11111111 })]
    [InlineData(42, new byte[] { 0b_00101010 })]
    [InlineData(-42, new byte[] { 0b_11010110 })]
    public void WriteSByte(sbyte data, byte[] expected)
    {
        Span<byte> buffer = stackalloc byte[expected.Length];
        buffer.Write(data);

        Assert.Equal(expected, buffer.ToArray());
    }

    [Theory]
    [InlineData(false, new byte[] { 0b_00000000 })]
    [InlineData(true, new byte[] { 0b_00000001 })]
    public void WriteBool(bool data, byte[] expected)
    {
        Span<byte> buffer = stackalloc byte[expected.Length];
        buffer.Write(data);

        Assert.Equal(expected, buffer.ToArray());
    }

    [Theory]
    [InlineData(0, new byte[] { 0b_00000000, 0b_00000000 })]
    [InlineData(42, new byte[] { 0b_00000000, 0b_00101010 })]
    [InlineData(255, new byte[] { 0b_00000000, 0b_11111111 })]
    [InlineData(256, new byte[] { 0b_00000001, 0b_00000000 })]
    [InlineData(ushort.MaxValue, new byte[] { 0b_11111111, 0b_11111111 })]
    public void WriteUShort(ushort data, byte[] expected)
    {
        Span<byte> buffer = stackalloc byte[expected.Length];
        buffer.Write(data);

        Assert.Equal(expected, buffer.ToArray());
    }

    [Theory]
    [InlineData(0, new byte[] { 0b_00000000, 0b_00000000 })]
    [InlineData(1, new byte[] { 0b_00000000, 0b_00000001 })]
    [InlineData(-1, new byte[] { 0b_11111111, 0b_11111111 })]
    [InlineData(42, new byte[] { 0b_00000000, 0b_00101010 })]
    [InlineData(-42, new byte[] { 0b_11111111, 0b_11010110 })]
    public void WriteShort(short data, byte[] expected)
    {
        Span<byte> buffer = stackalloc byte[expected.Length];
        buffer.Write(data);

        Assert.Equal(expected, buffer.ToArray());
    }

    [Theory]
    [InlineData(0, new byte[] { 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000 })]
    [InlineData(1, new byte[] { 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000001 })]
    [InlineData(42, new byte[] { 0b_00000000, 0b_00000000, 0b_00000000, 0b_00101010 })]
    [InlineData(uint.MaxValue, new byte[] { 0b_11111111, 0b_11111111, 0b_11111111, 0b_11111111 })]
    public void WriteUInt(uint data, byte[] expected)
    {
        Span<byte> buffer = stackalloc byte[expected.Length];
        buffer.Write(data);

        Assert.Equal(expected, buffer.ToArray());
    }

    [Theory]
    [InlineData(0, new byte[] { 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000 })]
    [InlineData(1, new byte[] { 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000001 })]
    [InlineData(-1, new byte[] { 0b_11111111, 0b_11111111, 0b_11111111, 0b_11111111 })]
    [InlineData(42, new byte[] { 0b_00000000, 0b_00000000, 0b_00000000, 0b_00101010 })]
    [InlineData(-42, new byte[] { 0b_11111111, 0b_11111111, 0b_11111111, 0b_11010110 })]
    public void WriteInt(int data, byte[] expected)
    {
        Span<byte> buffer = stackalloc byte[expected.Length];
        buffer.Write(data);

        Assert.Equal(expected, buffer.ToArray());
    }

    [Theory]
    [InlineData(0, new byte[] { 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000 })]
    [InlineData(1, new byte[] { 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000001 })]
    [InlineData(42, new byte[] { 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00101010 })]
    [InlineData(ulong.MaxValue, new byte[] { 0b_11111111, 0b_11111111, 0b_11111111, 0b_11111111, 0b_11111111, 0b_11111111, 0b_11111111, 0b_11111111 })]

    public void WriteULong(ulong data, byte[] expected)
    {
        Span<byte> buffer = stackalloc byte[expected.Length];
        buffer.Write(data);
            
        Assert.Equal(expected, buffer.ToArray());
    }

    [Theory]
    [InlineData(0, new byte[] { 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000 })]
    [InlineData(1, new byte[] { 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000001 })]
    [InlineData(-1, new byte[] { 0b_11111111, 0b_11111111, 0b_11111111, 0b_11111111, 0b_11111111, 0b_11111111, 0b_11111111, 0b_11111111 })]
    [InlineData(42, new byte[] { 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00000000, 0b_00101010 })]
    [InlineData(-42, new byte[] { 0b_11111111, 0b_11111111, 0b_11111111, 0b_11111111, 0b_11111111, 0b_11111111, 0b_11111111, 0b_11010110 })]
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
    [InlineData(null, false, new byte[] { 0 })]
    [InlineData("", false, new byte[] { 0 })]
    [InlineData("Lorem ipsu", false, new byte[] { 10, 076, 111, 114, 101, 109, 032, 105, 112, 115, 117 })]
    [InlineData("Лорем", false, new byte[] { 10, 208, 155, 208, 190, 209, 128, 208, 181, 208, 188 })]
    [InlineData("ლოaე", false, new byte[] { 10, 225, 131, 154, 225, 131, 157, 097, 225, 131, 148 })]
    [InlineData("片+目表", false, new byte[] { 10, 231, 137, 135, 043, 231, 155, 174, 232, 161, 168 })]
    [InlineData(null, true, new byte[] { 0, 0, 0, 0 })]
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
    [MemberData(nameof(WriteShortStringTooLongTestCases))]
    public void WriteThrowOnTooLongString(string text)
    {
        Assert.Throws<ArgumentException>(() =>
        {
            Span<byte> buffer = stackalloc byte[1000];
            buffer.Write(text, false);
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

    public static IEnumerable<object[]> WriteShortStringTooLongTestCases()
    {
        foreach (var text in Helpers.GetDummyTexts(256))
        {
            yield return new[] { text.Text };
        }
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