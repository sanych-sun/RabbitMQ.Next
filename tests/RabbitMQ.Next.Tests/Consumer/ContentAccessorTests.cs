using System;
using System.Buffers;
using NSubstitute;
using RabbitMQ.Next.Consumer;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Serialization.Abstractions;
using Xunit;

namespace RabbitMQ.Next.Tests.Consumer
{
    public class ContentAccessorTests
    {
        [Fact]
        public void ThrowsOnNullSerializerFactory()
        {
            Assert.Throws<ArgumentNullException>(() => new ContentAccessor(null));
        }


        [Fact]
        public void CanGetContent()
        {
            var data = new ReadOnlySequence<byte>(new byte[] {0x01, 0x02});
            var serializer = Substitute.For<ISerializer>();
            serializer.Deserialize<string>(Arg.Any<ReadOnlySequence<byte>>()).Returns("OK");
            var serializerFactory = Substitute.For<ISerializerFactory>();
            serializerFactory.Get(Arg.Any<string>()).Returns(serializer);

            var content = new ContentAccessor(serializerFactory);
            content.Set(data, "some-type");

            var result = content.GetContent<string>();

            serializerFactory.Received().Get("some-type");
            serializer.Received().Deserialize<string>(data);
            Assert.Equal("OK", result);
        }

        [Fact]
        public void ThrowsOnNoContentSet()
        {
            var serializerFactory = Substitute.For<ISerializerFactory>();

            var content = new ContentAccessor(serializerFactory);

            Assert.Throws<InvalidOperationException>(() => content.GetContent<string>());
        }

        [Fact]
        public void CanReset()
        {
            var data = new ReadOnlySequence<byte>(new byte[] {0x01, 0x02});
            var serializerFactory = Substitute.For<ISerializerFactory>();

            var content = new ContentAccessor(serializerFactory);
            content.Set(data, "some-type");

            content.Reset();
            Assert.Throws<InvalidOperationException>(() => content.GetContent<string>());
        }

        [Fact]
        public void ThrowsOnUnknownContentType()
        {
            var data = new ReadOnlySequence<byte>(new byte[] {0x01, 0x02});
            var serializerFactory = Substitute.For<ISerializerFactory>();
            serializerFactory.Get(Arg.Any<string>()).Returns((ISerializer)null);

            var content = new ContentAccessor(serializerFactory);
            content.Set(data, "another-type");

            Assert.Throws<NotSupportedException>(() => content.GetContent<string>());
        }
    }
}