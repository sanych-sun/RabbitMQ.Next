using System;
using System.Collections.Generic;
using RabbitMQ.Next.Abstractions.Messaging;
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

        [Theory]
        [MemberData(nameof(WriteMessagePropertiesTestCases))]
        public void WriteMessageProperties(byte[] expected, MessageProperties props)
        {
            Span<byte> buffer = stackalloc byte[expected.Length];

            buffer.WriteMessageProperties(props);

            Assert.Equal(expected, buffer.ToArray());
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

        public static IEnumerable<object[]> WriteMessagePropertiesTestCases()
        {
            yield return new object[]
            {
                new byte[] { 0b_00000000, 0b_00000000 },
                new MessageProperties()
            };

            yield return new object[]
            {
                new byte[] { 0b_10000000, 0b_00000000, 0x04, 0x6A, 0x73, 0x6F, 0x6E },
                new MessageProperties { ContentType = "json" }
            };

            yield return new object[]
            {
                new byte[] { 0b_01000000, 0b_00000000, 0x04, 0x75, 0x74, 0x66, 0x38 },
                new MessageProperties { ContentEncoding = "utf8" }
            };

            yield return new object[]
            {
                new byte[] { 0b_00100000, 0b_00000000, 0x00, 0x00, 0x00, 0x0E, 0x03, 0x6B, 0x65, 0x79, 0x53, 0x00, 0x00, 0x00, 0x05, 0x76, 0x61, 0x6C, 0x75, 0x65 },
                new MessageProperties { Headers = new Dictionary<string, object> {["key"] = "value"} }
            };

            yield return new object[]
            {
                new byte[] { 0b_00010000, 0b_00000000, 0x02 },
                new MessageProperties { DeliveryMode = DeliveryMode.Persistent }
            };

            yield return new object[]
            {
                new byte[] { 0b_00001000, 0b_00000000, 0x05 },
                new MessageProperties { Priority = 5 }
            };

            yield return new object[]
            {
                new byte[] { 0b_00000100, 0b_00000000, 0x0D, 0x63, 0x6F, 0x72, 0x72, 0x65, 0x6C, 0x61, 0x74, 0x69, 0x6F, 0x6E, 0x49, 0x64 },
                new MessageProperties { CorrelationId = "correlationId" }
            };

            yield return new object[]
            {
                new byte[] { 0b_00000010, 0b_00000000, 0x07, 0x72, 0x65, 0x70, 0x6C, 0x79, 0x54, 0x6F },
                new MessageProperties { ReplyTo = "replyTo" }
            };

            yield return new object[]
            {
                new byte[] { 0b_00000001, 0b_00000000, 0x0A, 0x65, 0x78, 0x70, 0x69, 0x72, 0x61, 0x74, 0x69, 0x6F, 0x6E },
                new MessageProperties { Expiration = "expiration" }
            };

            yield return new object[]
            {
                new byte[] { 0b_00000000, 0b_10000000, 0x09, 0x6D, 0x65, 0x73, 0x73, 0x61, 0x67, 0x65, 0x49, 0x64 },
                new MessageProperties { MessageId = "messageId" }
            };

            yield return new object[]
            {
                new byte[] { 0b_00000000, 0b_01000000, 0x00, 0x00, 0x00, 0x00, 0x19, 0xD7, 0x87, 0x00 },
                new MessageProperties { Timestamp = new DateTimeOffset(1983, 09,28, 0, 0, 0, TimeSpan.Zero) }
            };

            yield return new object[]
            {
                new byte[] { 0b_00000000, 0b_00100000, 0x04, 0x74, 0x79, 0x70, 0x65 },
                new MessageProperties { Type = "type" }
            };

            yield return new object[]
            {
                new byte[] { 0b_00000000, 0b_00010000, 0x06, 0x75, 0x73, 0x65, 0x72, 0x49, 0x64 },
                new MessageProperties { UserId = "userId" }
            };

            yield return new object[]
            {
                new byte[] { 0b_00000000, 0b_00001000, 0x05, 0x61, 0x70, 0x70, 0x49, 0x64 },
                new MessageProperties { ApplicationId = "appId" }
            };

            yield return new object[]
            {
                new byte[] { 0b_11111111, 0b_11111000,
                    0x04, 0x6A, 0x73, 0x6F, 0x6E,
                    0x04, 0x75, 0x74, 0x66, 0x38,
                    0x00, 0x00, 0x00, 0x0E, 0x03, 0x6B, 0x65, 0x79, 0x53, 0x00, 0x00, 0x00, 0x05, 0x76, 0x61, 0x6C, 0x75, 0x65,
                    0x02,
                    0x05,
                    0x0D, 0x63, 0x6F, 0x72, 0x72, 0x65, 0x6C, 0x61, 0x74, 0x69, 0x6F, 0x6E, 0x49, 0x64,
                    0x07, 0x72, 0x65, 0x70, 0x6C, 0x79, 0x54, 0x6F,
                    0x0A, 0x65, 0x78, 0x70, 0x69, 0x72, 0x61, 0x74, 0x69, 0x6F, 0x6E,
                    0x09, 0x6D, 0x65, 0x73, 0x73, 0x61, 0x67, 0x65, 0x49, 0x64,
                    0x00, 0x00, 0x00, 0x00, 0x19, 0xD7, 0x87, 0x00,
                    0x04, 0x74, 0x79, 0x70, 0x65,
                    0x06, 0x75, 0x73, 0x65, 0x72, 0x49, 0x64,
                    0x05, 0x61, 0x70, 0x70, 0x49, 0x64
                },
                new MessageProperties
                {
                    ContentType = "json",
                    ContentEncoding = "utf8",
                    Headers = new Dictionary<string, object> {["key"] = "value"},
                    DeliveryMode = DeliveryMode.Persistent,
                    Priority = 5,
                    CorrelationId = "correlationId",
                    ReplyTo = "replyTo",
                    Expiration = "expiration",
                    MessageId = "messageId",
                    Timestamp = new DateTimeOffset(1983, 09,28, 0, 0, 0, TimeSpan.Zero),
                    Type = "type",
                    UserId = "userId",
                    ApplicationId = "appId"
                }
            };
        }
    }
}