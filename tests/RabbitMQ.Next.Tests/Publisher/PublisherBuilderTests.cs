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

        [Fact]
        public void ConfirmsDefault()
        {
            var builder = new PublisherBuilder();

            Assert.False(builder.PublisherConfirms);
        }

        [Fact]
        public void Confirms()
        {
            var builder = new PublisherBuilder();
            ((IPublisherBuilder)builder).PublisherConfirms();

            Assert.True(builder.PublisherConfirms);
        }
    }
}