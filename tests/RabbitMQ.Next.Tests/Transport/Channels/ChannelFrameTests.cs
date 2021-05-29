using System.Buffers;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Transport.Channels;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Channels
{
    public class ChannelFrameTests
    {
        [Theory]
        [InlineData(ChannelFrameType.Unknown, 0, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 })]
        [InlineData(ChannelFrameType.Method, 321, new byte[] { 0x01, 0x00, 0x00, 0x01, 0x41 })]
        [InlineData(ChannelFrameType.Content, 42521, new byte[] { 0x02, 0x00, 0x00, 0xA6, 0x19 })]
        internal void WriteHeaderTests(ChannelFrameType type, uint size, byte[] expected)
        {
            var buffer = new ArrayBufferWriter<byte>(10);

            ChannelFrame.WriteHeader(buffer, type, size);

            Assert.Equal(expected, buffer.WrittenMemory.ToArray());
        }

        [Theory]
        [InlineData(ChannelFrameType.Unknown, 0, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 })]
        [InlineData(ChannelFrameType.Unknown, 0, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 }, 3)]
        [InlineData(ChannelFrameType.Method, 321, new byte[] { 0x01, 0x00, 0x00, 0x01, 0x41 })]
        [InlineData(ChannelFrameType.Method, 321, new byte[] { 0x01, 0x00, 0x00, 0x01, 0x41 }, 2)]
        [InlineData(ChannelFrameType.Content, 42521, new byte[] { 0x02, 0x00, 0x00, 0xA6, 0x19 })]
        [InlineData(ChannelFrameType.Content, 42521, new byte[] { 0x02, 0x00, 0x00, 0xA6, 0x19 }, 4)]
        internal void ReadHeaderTests(ChannelFrameType type, uint size, byte[] bytes, params int[] parts)
        {
            var buffer = Helpers.MakeSequence(bytes, parts);

            var header = ChannelFrame.ReadHeader(buffer);

            Assert.Equal(type, header.Type);
            Assert.Equal(size, header.Size);
        }
    }
}