using System;
using System.Buffers;
using NSubstitute;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Serialization.Abstractions;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher
{
    public class ContentAccessorTests
    {
        [Fact]
        public void ThrowsOnNullSerializer()
        {
            Assert.Throws<ArgumentNullException>(() => new ContentAccessor(null));
        }

        [Fact]
        public void CanGetContent()
        {
            var data = new ReadOnlySequence<byte>(new byte[] {0x01, 0x02});
            var serializer = Substitute.For<ISerializer>();
            serializer.Deserialize<string>(Arg.Any<ReadOnlySequence<byte>>()).Returns("OK");

            var content = new ContentAccessor(serializer);
            content.Set(data);

            var result = content.GetContent<string>();

            serializer.Received().Deserialize<string>(data);
            Assert.Equal("OK", result);
        }
    }
}