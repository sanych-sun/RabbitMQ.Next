using System;
using NSubstitute;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;
using RabbitMQ.Next.Publisher.Attributes;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Attributes
{
    public class PriorityAttributeTests
    {
        [Theory]
        [InlineData(10)]
        public void PriorityAttribute(byte value)
        {
            var attr = new PriorityAttribute(value);
            Assert.Equal(value, attr.Priority);
        }

        [Theory]
        [InlineData(42, null)]
        public void CanTransform(byte value, byte? builderValue)
        {
            var attr = new PriorityAttribute(value);
            var builder = Substitute.For<IMessageBuilder>();
            builder.Priority.Returns(builderValue);

            attr.Apply(builder);

            builder.Received().SetPriority(value);
        }

        [Theory]
        [InlineData(42, (byte)0)]
        [InlineData(42, (byte)5)]
        public void DoesNotOverrideExistingValue(byte value, byte? builderValue)
        {
            var attr = new PriorityAttribute(value);
            var builder = Substitute.For<IMessageBuilder>();
            builder.Priority.Returns(builderValue);

            attr.Apply(builder);

            builder.DidNotReceive().SetPriority(Arg.Any<byte>());
        }
    }
}