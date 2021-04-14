using System;
using RabbitMQ.Next.Transport.Messaging;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Messaging
{
    public class StringPropertyTests
    {
        [Theory]
        [InlineData(new byte[0], null)]
        [InlineData(new byte[] {0}, "")]
        [InlineData(new byte[] {10, 076, 111, 114, 101, 109, 032, 105, 112, 115, 117}, "Lorem ipsu")]
        public void CanParse(byte[] data, string expected)
        {
            var prop = new StringProperty(data);

            Assert.Equal(expected, prop.Value);
        }

        [Fact]
        public void ShouldNotParseTwice()
        {
            var prop = new StringProperty(new byte[] {10, 076, 111, 114, 101, 109, 032, 105, 112, 115, 117});

            Assert.Same(prop.Value, prop.Value);
        }
    }
}