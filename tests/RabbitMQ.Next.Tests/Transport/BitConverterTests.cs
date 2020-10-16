using RabbitMQ.Next.Transport;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport
{
    public class BitConverterTests
    {
        [Theory]
        [InlineData(0b_00000000, false)]
        [InlineData(0b_00000001, true)]
        [InlineData(0b_00000010, false, true)]
        [InlineData(0b_00000100, false, false, true)]
        [InlineData(0b_00001000, false, false, false, true)]
        [InlineData(0b_00010000, false, false, false, false, true)]
        [InlineData(0b_00100000, false, false, false, false, false, true)]
        [InlineData(0b_01000000, false, false, false, false, false, false, true)]
        [InlineData(0b_10000000, false, false, false, false, false, false, false, true)]
        [InlineData(0b_11111111, true, true, true, true, true, true, true, true)]
        public void ComposeFlags(
            byte expected,
            bool bit1, bool bit2 = false, bool bit3 = false, bool bit4 = false,
            bool bit5 = false, bool bit6 = false, bool bit7 = false, bool bit8 = false)
        {
            var result = BitConverter.ComposeFlags(bit1, bit2, bit3, bit4, bit5, bit6, bit7, bit8);

            Assert.Equal(expected, result);
        }
    }
}