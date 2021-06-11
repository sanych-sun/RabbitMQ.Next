using RabbitMQ.Next.Channels;
using Xunit;

namespace RabbitMQ.Next.Tests.Channels
{
    public class ReadOnlySequenceExtensionsTests
    {
        [Theory]
        [InlineData(0, new byte[] {0x00, 0x00, 0x00, 0x00})]
        [InlineData(0, new byte[] {0x00, 0x00}, new byte[] {0x00, 0x00})]
        [InlineData(0, new byte[] {0x00, 0x00}, new byte[] {0x00, 0x00, 0x00, 0x00})]
        [InlineData(1234, new byte[] {0x00, 0x00}, new byte[] {0x04, 0xD2})]
        [InlineData(1234, new byte[] {0x00, 0x00}, new byte[] {0x04, 0xD2, 0x00, 0x00})]
        public void ReadUint(uint expected, params byte[][] data)
        {
            var sequence = Helpers.MakeSequence(data);
            sequence.Read(out uint result);

            Assert.Equal(expected, result);
        }
    }
}