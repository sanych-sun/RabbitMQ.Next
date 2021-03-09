using System;
using NSubstitute;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Serialization.Abstractions;
using Xunit;

namespace RabbitMQ.Next.Tests.Serialization
{
    public class FormatterSourceTests
    {
        [Fact]
        public void ThrowsOnNullFormatter()
        {
            Assert.Throws<ArgumentNullException>(() => new FormatterSource(null));
        }

        [Fact]
        public void ReturnsCompatibleFormatter()
        {
            var formatter = Substitute.For<ITypeFormatter>();
            formatter.CanHandle(typeof(int)).Returns(true);
            formatter.CanHandle(typeof(long)).Returns(true);

            var source = new FormatterSource(formatter);

            Assert.True(source.TryGetFormatter<int>(out var fr));
            Assert.Equal(formatter, fr);

            Assert.True(source.TryGetFormatter<long>(out var fr2));
            Assert.Equal(formatter, fr2);
        }

        [Fact]
        public void DoesMotReturnNonCompatibleFormatter()
        {
            var formatter = Substitute.For<ITypeFormatter>();
            formatter.CanHandle(typeof(int)).Returns(true);
            formatter.CanHandle(typeof(long)).Returns(true);

            var source = new FormatterSource(formatter);

            Assert.False(source.TryGetFormatter<string>(out var fr));
            Assert.Null(fr);
        }
    }
}