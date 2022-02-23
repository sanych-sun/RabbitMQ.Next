using System;
using System.Collections.Generic;
using RabbitMQ.Next.Transport;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport
{
    public class FramingTests
    {
        [Theory]
        [MemberData(nameof(WriteFrameHeaderTestCases))]
        internal void WriteFrameHeader(FrameType type, ushort channel, uint size, byte[] expected)
        {
            var buffer = new byte[expected.Length];
            ((Span<byte>)buffer).WriteFrameHeader(type, channel, size);

            Assert.Equal(expected, buffer);
        }

        [Theory]
        [MemberData(nameof(ReadFrameHeaderTestCases))]
        internal void ReadFrameHeader(byte[] bytes, FrameType expectedType, ushort expectedChannel, uint expectedSize)
        {
            ((ReadOnlySpan<byte>)bytes).ReadFrameHeader(out FrameType type, out ushort channel, out uint size);

            Assert.Equal(expectedType, type);
            Assert.Equal(expectedChannel, channel);
            Assert.Equal(expectedSize, size);
        }

        public static IEnumerable<object[]> WriteFrameHeaderTestCases()
        {
            yield return new object[] { FrameType.Method, 1, 128, new byte[] { 1, 0, 1, 0, 0, 0, 128 } };
            yield return new object[] { FrameType.Heartbeat, 0, 3, new byte[] { 8, 0, 0, 0, 0, 0, 3 } };
            yield return new object[] { FrameType.ContentHeader, 2, 256, new byte[] { 2, 0, 2, 0, 0, 1, 0 } };
            yield return new object[] { FrameType.ContentBody, 3, 42, new byte[] { 3, 0, 3, 0, 0, 0, 42 } };
        }

        public static IEnumerable<object[]> ReadFrameHeaderTestCases()
        {
            yield return new object[] { new byte[] { 1, 0, 1, 0, 0, 0, 128 }, FrameType.Method, 1, 128 };
            yield return new object[] { new byte[] { 8, 0, 0, 0, 0, 0, 3 }, FrameType.Heartbeat, 0, 3 };
            yield return new object[] { new byte[] { 2, 0, 2, 0, 0, 1, 0 }, FrameType.ContentHeader, 2, 256 };
            yield return new object[] { new byte[] { 3, 0, 3, 0, 0, 0, 42 }, FrameType.ContentBody, 3, 42 };

            yield return new object[] { new byte[] { 0, 0, 3, 0, 0, 0, 42 }, FrameType.Malformed, 0, 0 };
            yield return new object[] { new byte[] { 11, 0, 3, 0, 0, 0, 42 }, FrameType.Malformed, 0, 0 };
        }
    }
}