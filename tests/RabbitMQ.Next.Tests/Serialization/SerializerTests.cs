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
            var mock = this.MockFormatterSource<string>();
            var serializer = new Serializer(mock.Source);

            serializer.Serialize("test", new ArrayBufferWriter<byte>());

            mock.Source.Received().TryGetFormatter<string>(out Arg.Any<IFormatter>());
            mock.Formatter.Received().Format("test", Arg.Any<IBufferWriter<byte>>());
        }

        [Fact]
        public void DeserializeCallFormatter()
        {
            var mock = this.MockFormatterSource<string>();
            var serializer = new Serializer(mock.Source);

            serializer.Deserialize<string>(ReadOnlySequence<byte>.Empty);

            mock.Source.Received().TryGetFormatter<string>(out Arg.Any<IFormatter>());
            mock.Formatter.Received().Parse<string>(Arg.Any<ReadOnlySequence<byte>>());
        }

        [Fact]
        public void SerializeThrowsOnNotSupportedTypes()
        {
            var mock = this.MockFormatterSource<string>();
            var serializer = new Serializer(mock.Source);

            Assert.Throws<InvalidOperationException>(() => serializer.Serialize(42, new ArrayBufferWriter<byte>()));
        }

        [Fact]
        public void DeserializeThrowsOnNotSupportedTypes()
        {
            var mock = this.MockFormatterSource<string>();
            var serializer = new Serializer(mock.Source);

            Assert.Throws<InvalidOperationException>(() => serializer.Deserialize<int>(ReadOnlySequence<byte>.Empty));
        }

        private (IFormatterSource Source, IFormatter Formatter) MockFormatterSource<TContent>()
        {
            var formatter = Substitute.For<IFormatter>();
            formatter.CanHandle(typeof(TContent)).Returns(true);

            var formatterSource = Substitute.For<IFormatterSource>();
            formatterSource.TryGetFormatter<TContent>(out Arg.Any<IFormatter>())
                .Returns(x => {
                    x[0] = formatter;
                    return true;
                });

            return (formatterSource, formatter);
        }
    }
}