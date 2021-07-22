using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Serialization.Abstractions;
using Xunit;

namespace RabbitMQ.Next.Tests.Serialization
{
    public class SerializerTests
    {
        [Theory]
        [MemberData(nameof(EmptyFormattersTestCases))]
        public void ThrowsOnEmptyFormatters(IEnumerable<ITypeFormatter> formatters)
        {
            Assert.Throws<ArgumentNullException>(() => new Serializer(formatters));
        }

        [Fact]
        public void SerializeThrowsOnNotSupportedTypes()
        {
            var serializer = new Serializer(new [] { this.MockFormatter<string>()});
            Assert.Throws<InvalidOperationException>(() => serializer.Serialize(42, new ArrayBufferWriter<byte>()));
        }

        [Fact]
        public void DeserializeThrowsOnNotSupportedTypes()
        {
            var serializer = new Serializer(new [] { this.MockFormatter<string>()});
            Assert.Throws<InvalidOperationException>(() => serializer.Deserialize<int>(ReadOnlySequence<byte>.Empty));
        }

        [Fact]
        public void SerializeChooseAppropriateFormatter()
        {
            var stringFormatter = this.MockFormatter<string>();
            var intFormatter = this.MockFormatter<int>();
            var buffer = Substitute.For<IBufferWriter<byte>>();
            var data = "test string";

            var serializer = new Serializer(new [] { intFormatter, stringFormatter });
            serializer.Serialize(data, buffer);

            stringFormatter.Received().Format(data, Arg.Any<IBufferWriter<byte>>());
        }

        [Fact]
        public void DeserializeChooseAppropriateFormatter()
        {
            var stringFormatter = this.MockFormatter<string>();
            var intFormatter = this.MockFormatter<int>();

            var serializer = new Serializer(new [] { intFormatter, stringFormatter });
            serializer.Deserialize<string>(ReadOnlySequence<byte>.Empty);

            stringFormatter.Received().Parse<string>(Arg.Any<ReadOnlySequence<byte>>());
        }

        private ITypeFormatter MockFormatter<TContent>()
        {
            var formatter = Substitute.For<ITypeFormatter>();
            formatter.CanHandle(typeof(TContent)).Returns(true);
            return formatter;
        }

        public static IEnumerable<object[]> EmptyFormattersTestCases()
        {
            yield return new object[] { null };
            yield return new object[] { Enumerable.Empty<ITypeFormatter>() };
            yield return new object[] { new ITypeFormatter[0] };
        }
    }
}