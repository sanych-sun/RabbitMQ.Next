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
        public void SerializeChooseAppropriateFormatter()
        {
            var stringFormatter = this.MockFormatter<string>();
            var intFormatter = this.MockFormatter<int>();
            var buffer = Substitute.For<IBufferWriter<byte>>();
            var data = "test string";

            var serializer = new PlainTextSerializer(new [] { intFormatter, stringFormatter });
            serializer.Serialize(data, buffer);

            stringFormatter.Received().Format(data, Arg.Any<IBufferWriter<byte>>());
        }

        [Fact]
        public void DeserializeChooseAppropriateFormatter()
        {
            var stringFormatter = this.MockFormatter<string>();
            var intFormatter = this.MockFormatter<int>();

            var serializer = new PlainTextSerializer(new [] { intFormatter, stringFormatter });
            serializer.Deserialize<string>(ReadOnlySequence<byte>.Empty);

            stringFormatter.Received().Parse<string>(Arg.Any<ReadOnlySequence<byte>>());
        }

        private IFormatter MockFormatter<TContent>()
        {
            var formatter = Substitute.For<IFormatter>();
            formatter.CanHandle(typeof(TContent)).Returns(true);
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