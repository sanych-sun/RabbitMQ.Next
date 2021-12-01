using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using RabbitMQ.Next.Serialization.PlainText;
using Xunit;

namespace RabbitMQ.Next.Tests.Serialization.PlainText
{
    public class PlainTextSerializerTests
    {
        [Theory]
        [MemberData(nameof(EmptyFormattersTestCases))]
        public void ThrowsOnEmptyFormatters(IEnumerable<IFormatter> formatters)
        {
            Assert.Throws<ArgumentNullException>(() => new PlainTextSerializer(formatters));
        }

        [Fact]
        public void SerializeThrowsOnNotSupportedTypes()
        {
            var serializer = new PlainTextSerializer(new [] { this.MockFormatter<string>()});
            Assert.Throws<InvalidOperationException>(() => serializer.Serialize(42, new ArrayBufferWriter<byte>()));
        }

        [Fact]
        public void DeserializeThrowsOnNotSupportedTypes()
        {
            var serializer = new PlainTextSerializer(new [] { this.MockFormatter<string>()});
            Assert.Throws<InvalidOperationException>(() => serializer.Deserialize<int>(ReadOnlySequence<byte>.Empty));
        }

        [Fact]
        public void SerializeTryFormattersUntilFound()
        {
            var intFormatter = this.MockFormatter<int>();
            var stringFormatter = this.MockFormatter<string>();
            var doubleFormatter = this.MockFormatter<double>();
            var buffer = Substitute.For<IBufferWriter<byte>>();
            var data = "test string";

            var serializer = new PlainTextSerializer(new [] { intFormatter, stringFormatter, doubleFormatter });
            serializer.Serialize(data, buffer);

            intFormatter.Received().TryFormat(data, Arg.Any<IBufferWriter<byte>>());
            stringFormatter.Received().TryFormat(data, Arg.Any<IBufferWriter<byte>>());
            doubleFormatter.DidNotReceive().TryFormat(data, Arg.Any<IBufferWriter<byte>>());
        }

        [Fact]
        public void DeserializeTryFormattersUntilFound()
        {
            var intFormatter = this.MockFormatter<int>();
            var stringFormatter = this.MockFormatter<string>();
            var doubleFormatter = this.MockFormatter<double>();

            var serializer = new PlainTextSerializer(new [] { intFormatter, stringFormatter, doubleFormatter });
            serializer.Deserialize<string>(ReadOnlySequence<byte>.Empty);

            intFormatter.Received().TryParse<string>(Arg.Any<ReadOnlySequence<byte>>(), out var _);
            stringFormatter.Received().TryParse<string>(Arg.Any<ReadOnlySequence<byte>>(), out var _);
            doubleFormatter.DidNotReceive().TryParse<string>(Arg.Any<ReadOnlySequence<byte>>(), out var _);
        }

        private IFormatter MockFormatter<TContent>()
        {
            var formatter = Substitute.For<IFormatter>();
            formatter.TryFormat<TContent>(Arg.Any<TContent>(), Arg.Any<IBufferWriter<byte>>()).Returns(true);
            formatter.TryParse<TContent>(Arg.Any<ReadOnlySequence<byte>>(), out Arg.Any<TContent>()).Returns(true);
            return formatter;
        }

        public static IEnumerable<object[]> EmptyFormattersTestCases()
        {
            yield return new object[] { null };
            yield return new object[] { Enumerable.Empty<IFormatter>() };
            yield return new object[] { new IFormatter[0] };
        }
    }
}