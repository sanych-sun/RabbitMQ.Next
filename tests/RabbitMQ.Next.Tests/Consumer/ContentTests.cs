using System;
using System.Buffers;
using NSubstitute;
using RabbitMQ.Next.Consumer.Abstractions;
using RabbitMQ.Next.Serialization.Abstractions;
using Xunit;

namespace RabbitMQ.Next.Tests.Consumer
{
    public class ContentTests
    {
        [Fact]
        public void ThrowsOnNullSerializer()
        {
            Assert.Throws<ArgumentNullException>(() => new Content(null, ReadOnlySequence<byte>.Empty));
        }

        [Fact]
        public void CanGetContent()
        {
            var data = new ReadOnlySequence<byte>(new byte[] {0x01, 0x02});
            var serializer = Substitute.For<ISerializer>();
            serializer.Deserialize<string>(Arg.Any<ReadOnlySequence<byte>>()).Returns("OK");

            var content = new Content(serializer, data);

            var result = content.GetContent<string>();

            serializer.Received().Deserialize<string>(data);
            Assert.Equal("OK", result);
        }

        [Fact]
        public void ThrowsOnDisposed()
        {
            var data = new ReadOnlySequence<byte>(new byte[] {0x01, 0x02});
            var serializer = Substitute.For<ISerializer>();

            var content = new Content(serializer, data);

            content.Dispose();
            Assert.Throws<ObjectDisposedException>(() => content.GetContent<string>());
        }

        [Fact]
        public void CanDisposeMultiple()
        {
            var data = new ReadOnlySequence<byte>(new byte[] {0x01, 0x02});
            var serializer = Substitute.For<ISerializer>();

            var content = new Content(serializer, data);

            content.Dispose();
            var exception = Record.Exception(() => content.Dispose());
            Assert.Null(exception);
        }
    }
}