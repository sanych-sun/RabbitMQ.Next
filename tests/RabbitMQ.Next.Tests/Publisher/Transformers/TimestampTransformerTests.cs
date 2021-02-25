using System;
using NSubstitute;
using RabbitMQ.Next.Publisher.Abstractions.Transformers;
using RabbitMQ.Next.Publisher.Transformers;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Transformers
{
    public class TimestampTransformerTests
    {
        [Fact]
        public void CanTransform()
        {
            var transformer = new TimestampTransformer();
            var message = Substitute.For<IMessageBuilder>();
            message.Timestamp.Returns((DateTimeOffset?)null);

            transformer.Apply(string.Empty, message);

            message.Received().SetTimestamp(Arg.Any<DateTimeOffset>());
        }

        [Fact]
        public void TimestampTransformerDoesNotOverride()
        {
            var transformer = new TimestampTransformer();
            var message = Substitute.For<IMessageBuilder>();
            message.Timestamp.Returns(DateTimeOffset.UtcNow);

            transformer.Apply(string.Empty, message);

            message.DidNotReceive().SetTimestamp(Arg.Any<DateTimeOffset>());
        }
    }
}