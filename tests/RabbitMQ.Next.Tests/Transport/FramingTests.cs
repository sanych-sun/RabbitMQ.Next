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
            Assert.Equal(ProtocolConstants.FrameHeaderSize, result);
        }

        [Theory]
        [MemberData(nameof(ReadFrameHeaderTestCases))]
        internal void ReadFrameHeader(byte[] bytes, FrameHeader expected)
        {
            var data = ((ReadOnlySpan<byte>)bytes).ReadFrameHeader();

            Assert.Equal(expected, data);
        }

        [Theory]
        [MemberData(nameof(WriteContentHeaderTestCases))]
        internal void writeContentHeader(byte[] expected, MessageProperties props, ulong size)
        {
            var buffer = new byte[expected.Length];
            var result = ((Span<byte>)buffer).WriteContentHeader(props, size);

            Assert.Equal(expected, buffer);
            Assert.Equal(expected.Length, result);
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

        public static IEnumerable<object[]> WriteContentHeaderTestCases()
        {
            yield return new object[]
            {
                new byte[] { 0, 60, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0b_00000000, 0b_00000000 },
                new MessageProperties(), 1
            };

            yield return new object[]
            {
                new byte[] {0, 60, 0, 0, 0, 0, 0, 0, 0, 0, 0, 42, 0b_10000000, 0b_00000000, 0x04, 0x6A, 0x73, 0x6F, 0x6E},
                new MessageProperties {ContentType = "json"}, 42
            };
        }
    }
}