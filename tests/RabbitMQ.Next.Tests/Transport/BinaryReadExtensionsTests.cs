using System;
using System.Collections.Generic;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Transport;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport
{
    public class BinaryReadExtensionsTests
    {
        [Theory]
        [InlineData(new byte[] { 0 }, 0, new byte[] { })]
        [InlineData(new byte[] { 1 }, 1, new byte[] { })]
        [InlineData(new byte[] { 214 }, 214, new byte[] { })]
        [InlineData(new byte[] { 1, 2, 3 }, 1, new byte[] { 2, 3 })]
        public void ReadByte(byte[] source, byte expectedData, byte[] expected)
        {
            var result = ((ReadOnlySpan<byte>)source).Read(out byte data);

            Assert.Equal(expectedData, data);
            Assert.Equal(expected, result.ToArray());
        }

        [Theory]
        [InlineData(new byte[] { 0 }, 0, new byte[] { })]
        [InlineData(new byte[] { 1 }, 1, new byte[] { })]
        [InlineData(new byte[] { 214 }, -42, new byte[] { })]
        [InlineData(new byte[] { 1, 2, 3 }, 1, new byte[] { 2, 3 })]
        public void ReadSByte(byte[] source, sbyte expectedData, byte[] expected)
        {
            var result = ((ReadOnlySpan<byte>)source).Read(out sbyte data);

            Assert.Equal(expectedData, data);
            Assert.Equal(expected, result.ToArray());
        }

        [Theory]
        [InlineData(new byte[] { 0 }, false, new byte[] { })]
        [InlineData(new byte[] { 1 }, true, new byte[] { })]
        [InlineData(new byte[] { 214 }, true, new byte[] { })]
        [InlineData(new byte[] { 1, 2, 3 }, true, new byte[] { 2, 3 })]
        public void ReadBool(byte[] source, bool expectedData, byte[] expected)
        {
            var result = ((ReadOnlySpan<byte>)source).Read(out bool data);

            Assert.Equal(expectedData, data);
            Assert.Equal(expected, result.ToArray());
        }

        [Theory]
        [InlineData(new byte[] { 0, 0 }, 0, new byte[] { })]
        [InlineData(new byte[] { 1, 0 }, 256, new byte[] { })]
        [InlineData(new byte[] { 214, 2 }, 54786, new byte[] { })]
        [InlineData(new byte[] { 1, 2, 3 }, 258, new byte[] { 3 })]
        public void ReadUShort(byte[] source, ushort expectedData, byte[] expected)
        {
            var result = ((ReadOnlySpan<byte>)source).Read(out ushort data);

            Assert.Equal(expectedData, data);
            Assert.Equal(expected, result.ToArray());
        }

        [Theory]
        [InlineData(new byte[] { 0, 0 }, 0, new byte[] { })]
        [InlineData(new byte[] { 1, 0 }, 256, new byte[] { })]
        [InlineData(new byte[] { 214, 2 }, -10750, new byte[] { })]
        [InlineData(new byte[] { 1, 2, 3 }, 258, new byte[] { 3 })]
        public void ReadShort(byte[] source, short expectedData, byte[] expected)
        {
            var result = ((ReadOnlySpan<byte>)source).Read(out short data);

            Assert.Equal(expectedData, data);
            Assert.Equal(expected, result.ToArray());
        }

        [Theory]
        [InlineData(new byte[] { 0, 0, 0, 0 }, 0, new byte[] { })]
        [InlineData(new byte[] { 1, 0, 0, 0 }, 16777216, new byte[] { })]
        [InlineData(new byte[] { 214, 0, 0, 0 }, 3590324224, new byte[] { })]
        [InlineData(new byte[] { 1, 0, 0, 0, 2 }, 16777216, new byte[] { 2 })]
        public void ReadUInt(byte[] source, uint expectedData, byte[] expected)
        {
            var result = ((ReadOnlySpan<byte>)source).Read(out uint data);

            Assert.Equal(expectedData, data);
            Assert.Equal(expected, result.ToArray());
        }

        [Theory]
        [InlineData(new byte[] { 0, 0, 0, 0 }, 0, new byte[] { })]
        [InlineData(new byte[] { 1, 0, 0, 0 }, 16777216, new byte[] { })]
        [InlineData(new byte[] { 214, 0, 0, 0 }, -704643072, new byte[] { })]
        [InlineData(new byte[] { 1, 0, 0, 0, 2 }, 16777216, new byte[] { 2 })]
        public void ReadInt(byte[] source, int expectedData, byte[] expected)
        {
            var result = ((ReadOnlySpan<byte>)source).Read(out int data);

            Assert.Equal(expectedData, data);
            Assert.Equal(expected, result.ToArray());
        }

        [Theory]
        [InlineData(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, 0, new byte[] { })]
        [InlineData(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }, 72057594037927936, new byte[] { })]
        [InlineData(new byte[] { 214, 0, 0, 0, 0, 0, 0, 0 }, 15420325124116578304, new byte[] { } )]
        [InlineData(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0, 2 }, 72057594037927936, new byte[] { 2 })]
        public void ReadULong(byte[] source, ulong expectedData, byte[] expected)
        {
            var result = ((ReadOnlySpan<byte>)source).Read(out ulong data);

            Assert.Equal(expectedData, data);
            Assert.Equal(expected, result.ToArray());
        }

        [Theory]
        [InlineData(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, 0, new byte[] { })]
        [InlineData(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }, 72057594037927936, new byte[] { })]
        [InlineData(new byte[] { 214, 0, 0, 0, 0, 0, 0, 0 }, -3026418949592973312, new byte[] { })]
        [InlineData(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0, 2 }, 72057594037927936, new byte[] { 2 })]
        public void ReadLong(byte[] source, long expectedData, byte[] expected)
        {
            var result = ((ReadOnlySpan<byte>)source).Read(out long data);

            Assert.Equal(expectedData, data);
            Assert.Equal(expected, result.ToArray());
        }

        [Theory]
        [InlineData(new byte[] { 0, 0, 0, 0 }, 0, new byte[] { })]
        [InlineData(new byte[] { 1, 0, 0, 0 }, 1E-45, new byte[] { })]
        [InlineData(new byte[] { 1, 0, 0, 0, 2 }, 1E-45, new byte[] { 2 })]
        public void ReadFloat(byte[] source, float expectedData, byte[] expected)
        {
            var result = ((ReadOnlySpan<byte>)source).Read(out float data);

            Assert.Equal(expectedData, data);
            Assert.Equal(expected, result.ToArray());
        }

        [Theory]
        [InlineData(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, 0, new byte[] { })]
        [InlineData(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }, 5E-324, new byte[] { })]
        [InlineData(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0, 2 }, 5E-324, new byte[] { 2 })]
        public void ReadDouble(byte[] source, double expectedData, byte[] expected)
        {
            var result = ((ReadOnlySpan<byte>)source).Read(out double data);

            Assert.Equal(expectedData, data);
            Assert.Equal(expected, result.ToArray());
        }

        [Theory]
        [InlineData(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 0, new byte[] { })]
        [InlineData(new byte[] { 0, 0, 2, 0, 0, 0, 0, 0, 32, 0, 0, 0, 0, 0, 0, 0 }, 0.32d, new byte[] { })]
        [InlineData(new byte[] { 0, 0, 2, 0, 0, 0, 0, 0, 32, 0, 0, 0, 0, 0, 0, 0, 2}, 0.32d, new byte[] { 2 })]
        public void ReadDecimal(byte[] source, decimal expectedData, byte[] expected)
        {
            var result = ((ReadOnlySpan<byte>)source).Read(out decimal data);

            Assert.Equal(expectedData, data);
            Assert.Equal(expected, result.ToArray());
        }

        [Theory]
        [InlineData(new byte[] { 0 }, false, "", new byte[] { })]
        [InlineData(new byte[] { 0, 2 }, false, "", new byte[] { 2 })]


        [InlineData(new byte[] { 10, 076, 111, 114, 101, 109, 032, 105, 112, 115, 117 }, false, "Lorem ipsu", new byte[] {} )]
        [InlineData(new byte[] { 10, 076, 111, 114, 101, 109, 032, 105, 112, 115, 117, 3, 5 }, false, "Lorem ipsu", new byte[] { 3, 5 } )]
        [InlineData(new byte[] { 10, 208, 155, 208, 190, 209, 128, 208, 181, 208, 188 }, false, "Лорем", new byte[] {} )]
        [InlineData(new byte[] { 10, 208, 155, 208, 190, 209, 128, 208, 181, 208, 188, 102, 12 }, false, "Лорем", new byte[] { 102, 12 } )]
        [InlineData(new byte[] { 10, 225, 131, 154, 225, 131, 157, 097, 225, 131, 148 }, false, "ლოaე", new byte[] {} )]
        [InlineData(new byte[] { 10, 225, 131, 154, 225, 131, 157, 097, 225, 131, 148, 5, 98 }, false, "ლოaე", new byte[] { 5, 98 } )]
        [InlineData(new byte[] { 10, 231, 137, 135, 043, 231, 155, 174, 232, 161, 168 }, false, "片+目表", new byte[] {} )]
        [InlineData(new byte[] { 10, 231, 137, 135, 043, 231, 155, 174, 232, 161, 168, 42, 1 }, false, "片+目表", new byte[] { 42, 1 } )]
        [InlineData(new byte[] { 0, 0, 0, 0 }, true, "", new byte[] { })]
        [InlineData(new byte[] { 0, 0, 0, 0, 2 }, true, "", new byte[] { 2 })]
        [InlineData(new byte[] { 0, 0, 0, 5, 72, 101, 108, 108, 111 }, true, "Hello", new byte[] { })]
        [InlineData(new byte[] { 0, 0, 0, 5, 72, 101, 108, 108, 111, 2 }, true, "Hello", new byte[] { 2 })]
        public void ReadString(byte[] source, bool isLongString, string expectedData, byte[] expected)
        {
            var result = ((ReadOnlySpan<byte>)source).Read(out string data, isLongString);

            Assert.Equal(expectedData, data);
            Assert.Equal(expected, result.ToArray());
        }

        [Theory]
        [InlineData(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, 0, new byte[] { })]
        [InlineData(new byte[] { 0, 0, 0, 0, 25, 215, 135, 0 }, 433555200, new byte[] { })]
        [InlineData(new byte[] { 0, 0, 0, 0, 25, 215, 135, 0, 2, 3 }, 433555200, new byte[] { 2, 3 })]
        public void ReadDateTime(byte[] source, long expectedData, byte[] expected)
        {
            var expectedDate = DateTimeOffset.FromUnixTimeSeconds(expectedData);
            var result = ((ReadOnlySpan<byte>)source).Read(out DateTimeOffset data);

            Assert.Equal(expectedDate, data);
            Assert.Equal(expected, result.ToArray());
        }

        [Theory]
        [InlineData(new byte[] { 0, 0, 0, 0 }, new byte[] { }, new byte[] { })]
        [InlineData(new byte[] { 0, 0, 0, 1, 1 }, new byte[] { 1 }, new byte[] { })]
        [InlineData(new byte[] { 0, 0, 0, 5, 1, 2, 3, 4, 5 }, new byte[] { 1, 2, 3, 4, 5 }, new byte[] { })]
        [InlineData(new byte[] { 0, 0, 0, 5, 1, 2, 3, 4, 5, 6 ,7 }, new byte[] { 1, 2, 3, 4, 5 }, new byte[] {6, 7})]
        public void ReadBinary(byte[] source, byte[] expectedData, byte[] expected)
        {
            var result = ((ReadOnlySpan<byte>)source).Read(out byte[] data);

            Assert.Equal(expectedData, data);
            Assert.Equal(expected, result.ToArray());
        }

        [Theory]
        [MemberData(nameof(ReadFieldTestCases))]
        public void ReadField(byte[] source, object expectedData, byte[] expected)
        {
            var result = ((ReadOnlySpan<byte>)source).ReadField(out object data);

            Assert.Equal(expectedData, data);
            Assert.Equal(expected, result.ToArray());
        }

        [Theory]
        [MemberData(nameof(ReadDictionaryTestCases))]
        public void ReadDictionary(byte[] source, IReadOnlyDictionary<string, object> expectedData, byte[] expected)
        {
            var result = ((ReadOnlySpan<byte>)source).Read(out IReadOnlyDictionary<string,object> data);

            Assert.Equal(expectedData, data);
            Assert.Equal(expected, result.ToArray());
        }

        [Theory]
        [MemberData(nameof(ReadArrayTestCases))]
        public void ReadArray(byte[] source, object[] expectedData, byte[] expected)
        {
            var result = ((ReadOnlySpan<byte>)source).Read(out object[] data);

            Assert.Equal(expectedData, data);
            Assert.Equal(expected, result.ToArray());
        }

        [Fact]
        public void ReadFieldThrowsOnUnknownTypePrefix()
        {
            var source = new byte[] {1, 2, 3};

            Assert.Throws<NotSupportedException>(() => ((ReadOnlySpan<byte>)source).ReadField(out _));
        }

        public static IEnumerable<object[]> ReadFieldTestCases()
        {
            yield return new object[] {new byte[] {116, 1}, true, new byte[] { }};
            yield return new object[] {new byte[] {116, 1, 1}, true, new byte[] {1}};
            yield return new object[] {new byte[] {66, 42}, (byte) 42, new byte[] { }};
            yield return new object[] {new byte[] {66, 42, 1}, (byte) 42, new byte[] {1}};
            yield return new object[] {new byte[] {98, 42}, (sbyte) 42, new byte[] { }};
            yield return new object[] {new byte[] {98, 42, 1}, (sbyte) 42, new byte[] {1}};
            yield return new object[] {new byte[] {115, 0, 42}, (short) 42, new byte[] { }};
            yield return new object[] {new byte[] {115, 0, 42, 1}, (short) 42, new byte[] {1}};
            yield return new object[] {new byte[] {105, 0, 0, 0, 42}, (uint) 42, new byte[] { }};
            yield return new object[] {new byte[] {105, 0, 0, 0, 42, 1}, (uint) 42, new byte[] {1}};
            yield return new object[] {new byte[] {73, 0, 0, 0, 42}, (int) 42, new byte[] { }};
            yield return new object[] {new byte[] {73, 0, 0, 0, 42, 1}, (int) 42, new byte[] {1}};
            yield return new object[] {new byte[] {108, 0, 0, 0, 0, 0, 0, 0, 42}, (long) 42, new byte[] { }};
            yield return new object[] {new byte[] {108, 0, 0, 0, 0, 0, 0, 0, 42, 1}, (long) 42, new byte[] {1}};
            yield return new object[] {new byte[] {102, 195, 245, 72, 64}, (float) 3.14, new byte[] { }};
            yield return new object[] {new byte[] {102, 195, 245, 72, 64, 1}, (float) 3.14, new byte[] {1}};
            yield return new object[] {new byte[] {100, 31, 133, 235, 81, 184, 30, 9, 64}, (double) 3.14, new byte[] { }};
            yield return new object[] {new byte[] {100, 31, 133, 235, 81, 184, 30, 9, 64, 1}, (double) 3.14, new byte[] {1}};
            yield return new object[] {new byte[] {68, 0, 0, 2, 0, 0, 0, 0, 0, 58, 1, 0, 0, 0, 0, 0, 0}, (decimal) 3.14, new byte[] { }};
            yield return new object[] {new byte[] {68, 0, 0, 2, 0, 0, 0, 0, 0, 58, 1, 0, 0, 0, 0, 0, 0, 1}, (decimal) 3.14, new byte[] {1}};
            yield return new object[] {new byte[] {84, 0, 0, 0, 0, 25, 215, 135, 0}, new DateTimeOffset(1983, 09, 28, 0, 0, 0, TimeSpan.Zero), new byte[] { }};
            yield return new object[] {new byte[] {84, 0, 0, 0, 0, 25, 215, 135, 0, 1}, new DateTimeOffset(1983, 09, 28, 0, 0, 0, TimeSpan.Zero), new byte[] {1}};
            yield return new object[] {new byte[] {83, 0, 0, 0, 5, 72, 101, 108, 108, 111}, "Hello", new byte[] { }};
            yield return new object[] {new byte[] {83, 0, 0, 0, 5, 72, 101, 108, 108, 111, 1}, "Hello", new byte[] {1}};
            yield return new object[] {new byte[] {65, 0, 0, 0, 17, 66, 42, 102, 195, 245, 72, 64, 83, 0, 0, 0, 5, 72, 101, 108, 108, 111}, new object[] {(byte) 42, (float) 3.14, "Hello"}, new byte[] { }};
            yield return new object[] {new byte[] {65, 0, 0, 0, 17, 66, 42, 102, 195, 245, 72, 64, 83, 0, 0, 0, 5, 72, 101, 108, 108, 111, 1}, new object[] {(byte) 42, (float) 3.14, "Hello"}, new byte[] {1}};
            yield return new object[] {new byte[] {70, 0, 0, 0, 14, 3, 107, 101, 121, 83, 0, 0, 0, 5, 118, 97, 108, 117, 101}, new Dictionary<string, object>() {["key"] = "value"}, new byte[] { }};
            yield return new object[] {new byte[] {70, 0, 0, 0, 14, 3, 107, 101, 121, 83, 0, 0, 0, 5, 118, 97, 108, 117, 101, 1}, new Dictionary<string, object>() {["key"] = "value"}, new byte[] {1}};
            yield return new object[] {new byte[] {120, 0, 0, 0, 5, 0, 1, 2, 3, 4}, new byte[] {0, 1, 2, 3, 4}, new byte[] { }};
            yield return new object[] {new byte[] {120, 0, 0, 0, 5, 0, 1, 2, 3, 4, 1}, new byte[] {0, 1, 2, 3, 4}, new byte[] {1}};
            yield return new object[] {new byte[] {86}, null, new byte[] { }};
            yield return new object[] {new byte[] {86, 1}, null, new byte[] {1}};
        }

        public static IEnumerable<object[]> ReadDictionaryTestCases()
        {
            yield return new object[] { new byte[] { 0, 0, 0, 0 }, null, new byte[] { } };
            yield return new object[] { new byte[] { 0, 0, 0, 0, 1 }, null, new byte[] { 1 } };
            yield return new object[] { new byte[] { 0, 0, 0, 14, 3, 107, 101, 121, 83, 0, 0, 0, 5, 118, 97, 108, 117, 101 }, new Dictionary<string, object> { ["key"] = "value" }, new byte[] { }};
            yield return new object[] { new byte[] { 0, 0, 0, 14, 3, 107, 101, 121, 83, 0, 0, 0, 5, 118, 97, 108, 117, 101, 1 }, new Dictionary<string, object> { ["key"] = "value" }, new byte[] { 1 }};
        }

        public static IEnumerable<object[]> ReadArrayTestCases()
        {
            yield return new object[] { new byte[] { 0, 0, 0, 0 }, null, new byte[] { }};
            yield return new object[] { new byte[] { 0, 0, 0, 0, 1 }, null, new byte[] {1}};
            yield return new object[] { new byte[] { 0, 0, 0, 17, 66, 42, 102, 195, 245, 72, 64, 83, 0, 0, 0, 5, 72, 101, 108, 108, 111 }, new object[] {(byte)42, (float)3.14, "Hello"}, new byte[] { }};
            yield return new object[] { new byte[] { 0, 0, 0, 17, 66, 42, 102, 195, 245, 72, 64, 83, 0, 0, 0, 5, 72, 101, 108, 108, 111, 1 }, new object[] {(byte)42, (float)3.14, "Hello"}, new byte[] {1}};
        }
    }
}