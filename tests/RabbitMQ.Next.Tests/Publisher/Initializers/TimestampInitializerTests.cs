using System;
using NSubstitute;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Publisher.Initializers;
using Xunit;

namespace RabbitMQ.Next.Tests.Publisher.Initializers
{
    public class TimestampInitializerTests
    {
        [Fact]
        public void CanTransform()
        {
            var transformer = new TimestampInitializer();
            var message = Substitute.For<IMessageBuilder>();

            transformer.Apply(string.Empty, message);

            message.Received().Timestamp(Arg.Any<DateTimeOffset>());
        }
    }
}