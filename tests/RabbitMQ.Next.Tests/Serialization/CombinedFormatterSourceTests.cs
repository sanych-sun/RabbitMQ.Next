using System;
using System.Collections.Generic;
using NSubstitute;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Serialization.Abstractions;
using Xunit;

namespace RabbitMQ.Next.Tests.Serialization
{
    public class CombinedFormatterSourceTests
    {
        [Fact]
        public void ThrowsOnNullFormatters()
        {
            Assert.Throws<ArgumentNullException>(() => new CombinedFormatterSource(null));
        }

        [Fact]
        public void ThrowsOnEmptyFormatters()
        {
            Assert.Throws<ArgumentException>(() => new CombinedFormatterSource(Array.Empty<IFormatterSource>()));
        }

        [Fact]
        public void ReturnsCompatibleFormatter()
        {
            var source = Mock(typeof(int), typeof(long));

            Assert.True(source.TryGetFormatter<int>(out var fr));
            Assert.NotNull(fr);

            Assert.True(source.TryGetFormatter<long>(out var fr2));
            Assert.NotNull(fr2);
        }

        [Fact]
        public void DoesMotReturnNonCompatibleFormatter()
        {
            var source = Mock(typeof(int), typeof(long));

            Assert.False(source.TryGetFormatter<string>(out var fr));
            Assert.Null(fr);
        }

        private CombinedFormatterSource Mock(params Type[] supportedTypes)
        {
            var formatters = new List<IFormatterSource>();

            foreach (var type in supportedTypes)
            {
                var fr = Substitute.For<ITypeFormatter>();
                fr.CanHandle(type).Returns(true);

                formatters.Add(new FormatterSource(fr));
            }

            return new CombinedFormatterSource(formatters);
        }
    }
}