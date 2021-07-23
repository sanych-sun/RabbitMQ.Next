using NSubstitute;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Publisher.Attributes;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Attributes
{
    public class ReplyToAttributeTests
    {
        [Theory]
        [InlineData("replyToQueue")]
        public void ReplyToAttribute(string value)
        {
            var attr = new ReplyToAttribute(value);
            Assert.Equal(value, attr.ReplyTo);
        }
        
        [Theory]
        [InlineData("replyToQueue", null)]
        [InlineData("replyToQueue", "")]
        public void CanTransform(string value, string builderValue)
        {
            var attr = new ReplyToAttribute(value);
            var builder = Substitute.For<IMessageBuilder>();
            builder.ReplyTo.Returns(builderValue);

            attr.Apply(builder);

            builder.Received().ReplyTo = value;
        }

        [Theory]
        [InlineData("replyToQueue", "value")]
        public void DoesNotOverrideExistingValue(string value, string builderValue)
        {
            var attr = new ReplyToAttribute(value);
            var builder = Substitute.For<IMessageBuilder>();
            builder.ReplyTo.Returns(builderValue);

            attr.Apply(builder);

            builder.DidNotReceive().ReplyTo = Arg.Any<string>();
        }
    }
}