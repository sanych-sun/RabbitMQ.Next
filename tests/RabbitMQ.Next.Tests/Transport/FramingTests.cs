using System;
using System.Collections.Generic;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Transport;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport
{
    public class FramingTests
    {
        [Fact]
        public void FrameHeaderCtor()
        {
            var type = FrameType.Method;
            var channel = (ushort)5;
            var payloadSize = 12345;
            var header = new FrameHeader(type, channel, payloadSize);

            Assert.Equal(type, header.Type);
            Assert.Equal(channel, header.Channel);
            Assert.Equal(payloadSize, header.PayloadSize);
        }

        [Theory]
        [MemberData(nameof(IsEmptyTestCases))]
        internal void IsEmpty(FrameHeader header, bool expected)
        {
            var result = header.IsEmpty();

            Assert.Equal(expected, result);
        }

        [Theory]
        [MemberData(nameof(WriteFrameHeaderTestCases))]
        internal void WriteFrameHeader(FrameHeader header, byte[] expected)
        {
            var buffer = new byte[expected.Length];
            var result = ((Span<byte>)buffer).WriteFrameHeader(header);

            Assert.Equal(expected, buffer);
            Assert.True(result.Length == 0);
        }

        [Theory]
        [MemberData(nameof(ReadFrameHeaderTestCases))]
        internal void ReadFrameHeader(byte[] bytes, FrameHeader expected)
        {
            var data = ((ReadOnlySpan<byte>)bytes).ReadFrameHeader();

            Assert.Equal(expected, data);
        }

        [Theory()]
        [MemberData(nameof(WriteMessagePropertiesTestCases))]
        public void WriteMessageProperties(byte[] expected, MessageProperties props)
        {
            Span<byte> buffer = stackalloc byte[expected.Length];

            buffer.WriteMessageProperties(props);

            Assert.Equal(expected, buffer.ToArray());
        }

        public static IEnumerable<object[]> IsEmptyTestCases()
        {
            yield return new object[] { default(FrameHeader), true };
            yield return new object[] { new FrameHeader(), true };
            yield return new object[] { new FrameHeader(FrameType.Method, 2, 3), false };
        }

        public static IEnumerable<object[]> WriteFrameHeaderTestCases()
        {
            yield return new object[] { new FrameHeader(FrameType.Method, 1, 128), new byte[] { 1, 0, 1, 0, 0, 0, 128 } };
            yield return new object[] { new FrameHeader(FrameType.Heartbeat, 0, 3), new byte[] { 8, 0, 0, 0, 0, 0, 3 } };
            yield return new object[] { new FrameHeader(FrameType.ContentHeader, 2, 256), new byte[] { 2, 0, 2, 0, 0, 1, 0 } };
            yield return new object[] { new FrameHeader(FrameType.ContentBody, 3, 42), new byte[] { 3, 0, 3, 0, 0, 0, 42 } };
        }

        public static IEnumerable<object[]> ReadFrameHeaderTestCases()
        {
            yield return new object[] { new byte[] { 1, 0, 1, 0, 0, 0, 128 }, new FrameHeader(FrameType.Method, 1, 128) };
            yield return new object[] { new byte[] { 8, 0, 0, 0, 0, 0, 3 }, new FrameHeader(FrameType.Heartbeat, 0, 3) };
            yield return new object[] { new byte[] { 2, 0, 2, 0, 0, 1, 0 }, new FrameHeader(FrameType.ContentHeader, 2, 256) };
            yield return new object[] { new byte[] { 3, 0, 3, 0, 0, 0, 42 }, new FrameHeader(FrameType.ContentBody, 3, 42) };

            yield return new object[] { new byte[] { 0, 0, 3, 0, 0, 0, 42 }, new FrameHeader(FrameType.Malformed, 0, 0) };
            yield return new object[] { new byte[] { 11, 0, 3, 0, 0, 0, 42 }, new FrameHeader(FrameType.Malformed, 0, 0) };
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
                new byte[] { 0b_01000000, 0b_00000000, 0x04, 0x6A, 0x73, 0x6F, 0x6E },
                new MessageProperties { ContentType = "json" }

            };

            yield return new object[]
            {
                new byte[] { 0b_00100000, 0b_00000000, 0x04, 0x75, 0x74, 0x66, 0x38 },
                new MessageProperties { ContentEncoding = "utf8" }
            };

            yield return new object[]
            {
                new byte[] { 0b_00010000, 0b_00000000, 0x00, 0x00, 0x00, 0x0E, 0x03, 0x6B, 0x65, 0x79, 0x53, 0x00, 0x00, 0x00, 0x05, 0x76, 0x61, 0x6C, 0x75, 0x65 },
                new MessageProperties { Headers = new Dictionary<string, object> {["key"] = "value"} }
            };

            yield return new object[]
            {
                new byte[] { 0b_00001000, 0b_00000000, 0x02 },
                new MessageProperties { DeliveryMode = DeliveryMode.Persistent }
            };

            yield return new object[]
            {
                new byte[] { 0b_00000100, 0b_00000000, 0x05 },
                new MessageProperties { Priority = 5 }
            };

            yield return new object[]
            {
                new byte[] { 0b_00000010, 0b_00000000, 0x0D, 0x63, 0x6F, 0x72, 0x72, 0x65, 0x6C, 0x61, 0x74, 0x69, 0x6F, 0x6E, 0x49, 0x64 },
                new MessageProperties { CorrelationId = "correlationId" }
            };

            yield return new object[]
            {
                new byte[] { 0b_00000001, 0b_00000000, 0x07, 0x72, 0x65, 0x70, 0x6C, 0x79, 0x54, 0x6F },
                new MessageProperties { ReplyTo = "replyTo" }
            };

            yield return new object[]
            {
                new byte[] { 0b_00000000, 0b_10000000, 0x0A, 0x65, 0x78, 0x70, 0x69, 0x72, 0x61, 0x74, 0x69, 0x6F, 0x6E },
                new MessageProperties { Expiration = "expiration" }
            };

            yield return new object[]
            {
                new byte[] { 0b_00000000, 0b_01000000, 0x09, 0x6D, 0x65, 0x73, 0x73, 0x61, 0x67, 0x65, 0x49, 0x64 },
                new MessageProperties { MessageId = "messageId" }
            };

            yield return new object[]
            {
                new byte[] { 0b_00000000, 0b_00100000, 0x00, 0x00, 0x00, 0x00, 0x19, 0xD7, 0x87, 0x00 },
                new MessageProperties { Timestamp = new DateTimeOffset(1983, 09,28, 0, 0, 0, TimeSpan.Zero) }
            };

            yield return new object[]
            {
                new byte[] { 0b_00000000, 0b_00010000, 0x04, 0x74, 0x79, 0x70, 0x65 },
                new MessageProperties { Type = "type" }
            };

            yield return new object[]
            {
                new byte[] { 0b_00000000, 0b_00001000, 0x06, 0x75, 0x73, 0x65, 0x72, 0x49, 0x64 },
                new MessageProperties { UserId = "userId" }
            };

            yield return new object[]
            {
                new byte[] { 0b_00000000, 0b_00000100, 0x05, 0x61, 0x70, 0x70, 0x49, 0x64 },
                new MessageProperties { ApplicationId = "appId" }
            };



            yield return new object[]
            {
                new byte[] { 0b_01111111, 0b_11111100,
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