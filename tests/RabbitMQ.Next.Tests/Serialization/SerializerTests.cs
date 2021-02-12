using System;
using System.Buffers;
using NSubstitute;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Serialization.Abstractions;
using Xunit;

namespace RabbitMQ.Next.Tests.Serialization
{
    public class SerializerTests
    {
        [Fact]
        public void SerializeCallFormatter()
        {
            this.MockFormatterSource<string>(out var formatterSource, out var formatter);
            var serializer = new Serializer(formatterSource);

            serializer.Serialize("test", new ArrayBufferWriter<byte>());

            formatterSource.Received().GetFormatter<string>();
            formatter.Received().Format("test", Arg.Any<IBufferWriter<byte>>());
        }

        [Fact]
        public void DeserializeCallFormatter()
        {
            this.MockFormatterSource<string>(out var formatterSource, out var formatter);
            var serializer = new Serializer(formatterSource);

            serializer.Deserialize<string>(ReadOnlySequence<byte>.Empty);

            formatterSource.Received().GetFormatter<string>();
            formatter.Received().Parse<string>(Arg.Any<ReadOnlySequence<byte>>());
        }

        [Fact]
        public void SerializeThrowsOnNotSupportedTypes()
        {
            var formatterSource = Substitute.For<IFormatterSource>();
            formatterSource.GetFormatter<string>().Returns((IFormatter)null);
            var serializer = new Serializer(formatterSource);

            Assert.Throws<InvalidOperationException>(() => serializer.Serialize("test", new ArrayBufferWriter<byte>()));
        }

        [Fact]
        public void DeserializeThrowsOnNotSupportedTypes()
        {
            var formatterSource = Substitute.For<IFormatterSource>();
            formatterSource.GetFormatter<string>().Returns((IFormatter)null);
            var serializer = new Serializer(formatterSource);

            Assert.Throws<InvalidOperationException>(() => serializer.Deserialize<string>(ReadOnlySequence<byte>.Empty));
        }

        private void MockFormatterSource<TContent>(out IFormatterSource formatterSource, out IFormatter formatter)
        {
            formatter = Substitute.For<IFormatter>();
            formatter.CanHandle(typeof(TContent)).Returns(true);

            formatterSource = Substitute.For<IFormatterSource>();
            formatterSource.GetFormatter<string>().Returns(formatter);
        }
    }
}