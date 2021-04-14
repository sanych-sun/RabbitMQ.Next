using System;
using System.Buffers;
using System.Collections.Generic;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Transport.Messaging;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Messaging
{
    public class MessagePropertiesTests
    {
        [Theory]
        [MemberData(nameof(MessagePropertiesTestCases))]
        public void ReadMessageProperties(byte[] source, IMessageProperties expectedData)
        {
            var props = new MessageProperties(new ReadOnlySequence<byte>(source));

            Assert.Equal(expectedData, props, new MessagePropertiesComparer());
        }

        [Theory]
        [MemberData(nameof(MessagePropertiesTestCases))]
        public void WriteMessageProperties(byte[] expected, IMessageProperties props)
        {
            Span<byte> buffer = stackalloc byte[expected.Length];

            buffer.WriteMessageProperties(props);

            Assert.Equal(expected, buffer.ToArray());
        }

        [Fact]
        public void CanDisposeMultipleTimes()
        {
            var props = new MessageProperties(new ReadOnlySequence<byte>(new byte[] { 0b_00000000, 0b_00000000 }));

            props.Dispose();

            var exception = Record.Exception(() => props.Dispose());
            Assert.Null(exception);
        }

        [Fact]
        public void ThrowsIfDisposed()
        {
            var props = new MessageProperties(new ReadOnlySequence<byte>(new byte[] { 0b_00000000, 0b_00000000 }));

            props.Dispose();

            Assert.Throws<ObjectDisposedException>(() => props.ContentType);
            Assert.Throws<ObjectDisposedException>(() => props.ContentEncoding);
            Assert.Throws<ObjectDisposedException>(() => props.Headers);
            Assert.Throws<ObjectDisposedException>(() => props.DeliveryMode);
            Assert.Throws<ObjectDisposedException>(() => props.Priority);
            Assert.Throws<ObjectDisposedException>(() => props.ReplyTo);
            Assert.Throws<ObjectDisposedException>(() => props.Expiration);
            Assert.Throws<ObjectDisposedException>(() => props.MessageId);
            Assert.Throws<ObjectDisposedException>(() => props.Timestamp);
            Assert.Throws<ObjectDisposedException>(() => props.Type);
            Assert.Throws<ObjectDisposedException>(() => props.UserId);
            Assert.Throws<ObjectDisposedException>(() => props.ApplicationId);
        }

        public static IEnumerable<object[]> MessagePropertiesTestCases()
        {
            yield return new object[]
            {
                new byte[] { 0b_00000000, 0b_00000000 },
                new MessagePropertiesMock()
            };

            yield return new object[]
            {
                new byte[] { 0b_10000000, 0b_00000000, 0x04, 0x6A, 0x73, 0x6F, 0x6E },
                new MessagePropertiesMock { ContentType = "json" }
            };

            yield return new object[]
            {
                new byte[] { 0b_01000000, 0b_00000000, 0x04, 0x75, 0x74, 0x66, 0x38 },
                new MessagePropertiesMock { ContentEncoding = "utf8" }
            };

            yield return new object[]
            {
                new byte[] { 0b_00100000, 0b_00000000, 0x00, 0x00, 0x00, 0x0E, 0x03, 0x6B, 0x65, 0x79, 0x53, 0x00, 0x00, 0x00, 0x05, 0x76, 0x61, 0x6C, 0x75, 0x65 },
                new MessagePropertiesMock { Headers = new Dictionary<string, object> {["key"] = "value"} }
            };

            yield return new object[]
            {
                new byte[] { 0b_00010000, 0b_00000000, 0x02 },
                new MessagePropertiesMock { DeliveryMode = DeliveryMode.Persistent }
            };

            yield return new object[]
            {
                new byte[] { 0b_00001000, 0b_00000000, 0x05 },
                new MessagePropertiesMock { Priority = 5 }
            };

            yield return new object[]
            {
                new byte[] { 0b_00000100, 0b_00000000, 0x0D, 0x63, 0x6F, 0x72, 0x72, 0x65, 0x6C, 0x61, 0x74, 0x69, 0x6F, 0x6E, 0x49, 0x64 },
                new MessagePropertiesMock { CorrelationId = "correlationId" }
            };

            yield return new object[]
            {
                new byte[] { 0b_00000010, 0b_00000000, 0x07, 0x72, 0x65, 0x70, 0x6C, 0x79, 0x54, 0x6F },
                new MessagePropertiesMock { ReplyTo = "replyTo" }
            };

            yield return new object[]
            {
                new byte[] { 0b_00000001, 0b_00000000, 0x0A, 0x65, 0x78, 0x70, 0x69, 0x72, 0x61, 0x74, 0x69, 0x6F, 0x6E },
                new MessagePropertiesMock { Expiration = "expiration" }
            };

            yield return new object[]
            {
                new byte[] { 0b_00000000, 0b_10000000, 0x09, 0x6D, 0x65, 0x73, 0x73, 0x61, 0x67, 0x65, 0x49, 0x64 },
                new MessagePropertiesMock { MessageId = "messageId" }
            };

            yield return new object[]
            {
                new byte[] { 0b_00000000, 0b_01000000, 0x00, 0x00, 0x00, 0x00, 0x19, 0xD7, 0x87, 0x00 },
                new MessagePropertiesMock { Timestamp = new DateTimeOffset(1983, 09,28, 0, 0, 0, TimeSpan.Zero) }
            };

            yield return new object[]
            {
                new byte[] { 0b_00000000, 0b_00100000, 0x04, 0x74, 0x79, 0x70, 0x65 },
                new MessagePropertiesMock { Type = "type" }
            };

            yield return new object[]
            {
                new byte[] { 0b_00000000, 0b_00010000, 0x06, 0x75, 0x73, 0x65, 0x72, 0x49, 0x64 },
                new MessagePropertiesMock { UserId = "userId" }
            };

            yield return new object[]
            {
                new byte[] { 0b_00000000, 0b_00001000, 0x05, 0x61, 0x70, 0x70, 0x49, 0x64 },
                new MessagePropertiesMock { ApplicationId = "appId" }
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
                new MessagePropertiesMock
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