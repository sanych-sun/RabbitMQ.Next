using System;
using NSubstitute;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;
using RabbitMQ.Next.Publisher.Attributes;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Attributes
{
    public class TimestampAttributeTests
    {
        [Fact]
        public void CanTransform()
        {
            var attr = new TimestampAttribute();
            var builder = Substitute.For<IMessageBuilder>();
            builder.Timestamp.Returns((DateTimeOffset?)null);

            attr.Apply(builder);

            builder.Received().SetTimestamp(Arg.Any<DateTimeOffset>());
        }

        [Fact]
        public void DoesNotOverrideExistingValue()
        {
            var attr = new TimestampAttribute();
            var builder = Substitute.For<IMessageBuilder>();
            builder.Timestamp.Returns(DateTimeOffset.UtcNow);

            attr.Apply(builder);

            builder.DidNotReceive().SetTimestamp(Arg.Any<DateTimeOffset>());
        }
    }
}