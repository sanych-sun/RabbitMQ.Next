using System;
using NSubstitute;
using RabbitMQ.Next.Serialization;
using RabbitMQ.Next.Serialization.Abstractions;
using Xunit;

namespace RabbitMQ.Next.Tests.Serialization
{
    public class FormatterSourceHelperTests
    {
        [Fact]
        public void CanCombineSingleFormatter()
        {
            var formatter = this.MockFormatter(typeof(int));
            var source = FormatterSourceHelper.CombineFormatters(new[] {formatter}, null);

            Assert.True(source.TryGetFormatter<int>(out var fr));
            Assert.Equal(formatter, fr);
        }

        [Fact]
        public void CanCombineMultipleFormatters()
        {
            var formatterInt = this.MockFormatter(typeof(int));
            var formatterString = this.MockFormatter(typeof(string));
            var source = FormatterSourceHelper.CombineFormatters(new[] {formatterInt, formatterString}, null);

            Assert.True(source.TryGetFormatter<int>(out var fr1));
            Assert.Equal(formatterInt, fr1);

            Assert.True(source.TryGetFormatter<string>(out var fr2));
            Assert.Equal(formatterString, fr2);
        }

        [Fact]
        public void FirstFormatterHasMorePriority()
        {
            var formatterInt = this.MockFormatter(typeof(int));
            var formatterInt2 = this.MockFormatter(typeof(int));
            var source = FormatterSourceHelper.CombineFormatters(new[] {formatterInt, formatterInt2}, null);

            Assert.True(source.TryGetFormatter<int>(out var fr1));
            Assert.Equal(formatterInt, fr1);
        }

        [Fact]
        public void CanCombineSource()
        {
            var sourceInt = this.MockFormatterSource<int>();
            var source = FormatterSourceHelper.CombineFormatters(null, new[] {sourceInt});

            Assert.True(source.TryGetFormatter<int>(out var fr1));
            Assert.NotNull(fr1);
        }

        [Fact]
        public void CanCombineMultipleSources()
        {
            var sourceInt = this.MockFormatterSource<int>();
            var sourceString = this.MockFormatterSource<string>();
            var source = FormatterSourceHelper.CombineFormatters(null, new[] {sourceInt, sourceString});

            Assert.True(source.TryGetFormatter<int>(out var fr1));
            Assert.NotNull(fr1);

            Assert.True(source.TryGetFormatter<string>(out var fr2));
            Assert.NotNull(fr2);
        }

        [Fact]
        public void CanCombineFormattersAndSources()
        {
            var formatterInt = this.MockFormatter(typeof(int));
            var formatterString = this.MockFormatter(typeof(string));

            var sourceDate = this.MockFormatterSource<DateTime>();
            var sourceLong = this.MockFormatterSource<long>();

            var source = FormatterSourceHelper.CombineFormatters(new[] {formatterInt, formatterString}, new[] {sourceDate, sourceLong});

            Assert.True(source.TryGetFormatter<int>(out var fr1));
            Assert.NotNull(fr1);

            Assert.True(source.TryGetFormatter<string>(out var fr2));
            Assert.NotNull(fr2);

            Assert.True(source.TryGetFormatter<DateTime>(out var fr3));
            Assert.NotNull(fr1);

            Assert.True(source.TryGetFormatter<long>(out var fr4));
            Assert.NotNull(fr2);
        }

        [Fact]
        public void FormatterHasMorePriority()
        {
            var formatterInt = this.MockFormatter(typeof(int));
            var sourceInt = this.MockFormatterSource<int>();

            var source = FormatterSourceHelper.CombineFormatters(new[] {formatterInt}, new[] {sourceInt});

            Assert.True(source.TryGetFormatter<int>(out var fr1));
            Assert.Equal(formatterInt, fr1);
        }

        private ITypeFormatter MockFormatter(params Type[] supportedTypes)
        {
            var formatter = Substitute.For<ITypeFormatter>();

            foreach (var type in supportedTypes)
            {
                formatter.CanHandle(type).Returns(true);
            }

            return formatter;
        }

        private IFormatterSource MockFormatterSource<T>()
        {
            var source = Substitute.For<IFormatterSource>();
            source.TryGetFormatter<T>(out Arg.Any<ITypeFormatter>())
                .Returns(x =>
                {
                    x[0] = this.MockFormatter(typeof(T));
                    return true;
                });

            return source;
        }
    }
}