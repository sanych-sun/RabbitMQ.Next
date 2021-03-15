using System;
using NSubstitute;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;
using RabbitMQ.Next.Serialization.Abstractions;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher
{
    public class PublisherBuilderTests
    {
        [Fact]
        public void BufferIsDisabledByDefault()
        {
            var builder = new PublisherBuilder();

            Assert.Equal(0, builder.BufferSize);
        }

        [Fact]
        public void CanSetBufferSize()
        {
            var bufferSize = 10;
            var builder = new PublisherBuilder();

            ((IPublisherBuilder) builder).AllowBuffer(bufferSize);

            Assert.Equal(bufferSize, builder.BufferSize);
        }

        [Fact]
        public void ThrowsOnInvalidBufferSize()
        {
            var builder = new PublisherBuilder();

            Assert.Throws<ArgumentOutOfRangeException>(() => ((IPublisherBuilder) builder).AllowBuffer(-1));
        }

        [Fact]
        public void CanRegisterFormatters()
        {
            var formatter1 = Substitute.For<ITypeFormatter>();
            var formatter2 = Substitute.For<ITypeFormatter>();

            var builder = new PublisherBuilder();
            ((IPublisherBuilder) builder).UseFormatter(formatter1);
            ((IPublisherBuilder) builder).UseFormatter(formatter2);

            Assert.Contains(formatter1, builder.Formatters);
            Assert.Contains(formatter2, builder.Formatters);
        }

        [Fact]
        public void ThrowsOnInvalidFormatter()
        {
            var builder = new PublisherBuilder();
            
            Assert.Throws<ArgumentNullException>(() => ((IPublisherBuilder)builder).UseFormatter(null));
        }
        
        [Fact]
        public void CanRegisterFormatterSources()
        {
            var formatter1 = Substitute.For<IFormatterSource>();
            var formatter2 = Substitute.For<IFormatterSource>();

            var builder = new PublisherBuilder();
            ((IPublisherBuilder) builder).UseFormatterSource(formatter1);
            ((IPublisherBuilder) builder).UseFormatterSource(formatter2);

            Assert.Contains(formatter1, builder.FormatterSources);
            Assert.Contains(formatter2, builder.FormatterSources);
        }

        [Fact]
        public void ThrowsOnInvalidFormatterSource()
        {
            var builder = new PublisherBuilder();
            
            Assert.Throws<ArgumentNullException>(() => ((IPublisherBuilder)builder).UseFormatterSource(null));
        }
        
        [Fact]
        public void CanRegisterTransformers()
        {
            var transformer1 = Substitute.For<IMessageTransformer>();
            var transformer2 = Substitute.For<IMessageTransformer>();

            var builder = new PublisherBuilder();
            ((IPublisherBuilder) builder).UseTransformer(transformer1);
            ((IPublisherBuilder) builder).UseTransformer(transformer2);

            Assert.Contains(transformer1, builder.Transformers);
            Assert.Contains(transformer2, builder.Transformers);
        }

        [Fact]
        public void ThrowsOnInvalidTransformer()
        {
            var builder = new PublisherBuilder();
            
            Assert.Throws<ArgumentNullException>(() => ((IPublisherBuilder)builder).UseTransformer(null));
        }
        
        [Fact]
        public void CanRegisterReturnedMessageHandlers()
        {
            var transformer1 = Substitute.For<IReturnedMessageHandler>();
            var transformer2 = Substitute.For<IReturnedMessageHandler>();

            var builder = new PublisherBuilder();
            ((IPublisherBuilder) builder).AddReturnedMessageHandler(transformer1);
            ((IPublisherBuilder) builder).AddReturnedMessageHandler(transformer2);

            Assert.Contains(transformer1, builder.ReturnedMessageHandlers);
            Assert.Contains(transformer2, builder.ReturnedMessageHandlers);
        }

        [Fact]
        public void ThrowsOnInvalidReturnedMessageHandler()
        {
            var builder = new PublisherBuilder();
            
            Assert.Throws<ArgumentNullException>(() => ((IPublisherBuilder)builder).AddReturnedMessageHandler(null));
        }
    }
}