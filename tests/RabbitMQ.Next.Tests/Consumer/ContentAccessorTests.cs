using System;
using System.Buffers;
using NSubstitute;
using RabbitMQ.Next.Abstractions.Messaging;
using RabbitMQ.Next.Consumer;
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
            serializerFactory.Get(Arg.Any<IMessageProperties>()).Returns(serializer);
            var messageProperties = Substitute.For<IMessageProperties>();

            var content = new ContentAccessor(serializerFactory);
            content.Set(data, messageProperties);

            var result = content.GetContent<string>();

            serializerFactory.Received().Get(messageProperties);
            serializer.Received().Deserialize<string>(data);
            Assert.Equal("OK", result);
        }

        [Fact]
        public void GetContentThrowsOnNoContentSet()
        {
            var serializerFactory = Substitute.For<ISerializerFactory>();

            var content = new ContentAccessor(serializerFactory);

            Assert.Throws<InvalidOperationException>(() => content.GetContent<string>());
        }

        [Fact]
        public void CanGetProperties()
        {
            var data = new ReadOnlySequence<byte>(new byte[] {0x01, 0x02});
            var serializer = Substitute.For<ISerializer>();
            serializer.Deserialize<string>(Arg.Any<ReadOnlySequence<byte>>()).Returns("OK");
            var serializerFactory = Substitute.For<ISerializerFactory>();
            serializerFactory.Get(Arg.Any<IMessageProperties>()).Returns(serializer);
            var messageProperties = Substitute.For<IMessageProperties>();

            var content = new ContentAccessor(serializerFactory);
            content.Set(data, messageProperties);

            Assert.Equal(messageProperties, content.Properties);
        }

        [Fact]
        public void PropertiesThrowsOnNoContentSet()
        {
            var serializerFactory = Substitute.For<ISerializerFactory>();

            var content = new ContentAccessor(serializerFactory);

            Assert.Throws<InvalidOperationException>(() => content.Properties);
        }


        [Fact]
        public void CanReset()
        {
            var data = new ReadOnlySequence<byte>(new byte[] {0x01, 0x02});
            var serializerFactory = Substitute.For<ISerializerFactory>();
            var messageProperties = Substitute.For<IMessageProperties>();

            var content = new ContentAccessor(serializerFactory);
            content.Set(data, messageProperties);

            content.Reset();
            Assert.Throws<InvalidOperationException>(() => content.GetContent<string>());
            Assert.Throws<InvalidOperationException>(() => content.Properties);
        }
    }
}