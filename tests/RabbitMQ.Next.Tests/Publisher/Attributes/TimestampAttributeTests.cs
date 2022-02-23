using System;
using NSubstitute;
using RabbitMQ.Next.Publisher;
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

            attr.Apply(builder);

            builder.Received().Timestamp(Arg.Any<DateTimeOffset>());
        }
    }
}