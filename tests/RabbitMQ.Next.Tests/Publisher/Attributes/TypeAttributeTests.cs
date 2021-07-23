using NSubstitute;
using RabbitMQ.Next.Publisher.Abstractions;
using RabbitMQ.Next.Publisher.Attributes;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Attributes
{
    public class TypeAttributeTests
    {
        [Theory]
        [InlineData("myType")]
        public void TypeAttribute(string value)
        {
            var attr = new TypeAttribute(value);
            Assert.Equal(value, attr.Type);
        }

        [Theory]
        [InlineData("myType", null)]
        [InlineData("myType", "")]
        public void CanTransform(string value, string builderValue)
        {
            var attr = new TypeAttribute(value);
            var builder = Substitute.For<IMessageBuilder>();
            builder.Type.Returns(builderValue);

            attr.Apply(builder);

            builder.Received().Type = value;
        }

        [Theory]
        [InlineData("myType", "value")]
        public void DoesNotOverrideExistingValue(string value, string builderValue)
        {
            var attr = new TypeAttribute(value);
            var builder = Substitute.For<IMessageBuilder>();
            builder.Type.Returns(builderValue);

            attr.Apply(builder);

            builder.DidNotReceive().Type = Arg.Any<string>();
        }
    }
}