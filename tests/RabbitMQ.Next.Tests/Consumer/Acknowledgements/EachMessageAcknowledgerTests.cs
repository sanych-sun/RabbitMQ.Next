using System;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Consumer.Abstractions.Acknowledgement;
using Xunit;

namespace RabbitMQ.Next.Tests.Consumer.Acknowledgements
{
    public class EachMessageAcknowledgerTests
    {
        [Fact]
        public void ThrowsOnNullAcknowledgement()
        {
            Assert.Throws<ArgumentNullException>(() => new EachMessageAcknowledger(null));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(12345)]
        public async Task AckPassParametersToBase(ulong deliveryTag)
        {
            var baseAck = Substitute.For<IAcknowledgement>();
            var ack = new EachMessageAcknowledger(baseAck);

            await ack.AckAsync(deliveryTag);

            await baseAck.Received().AckAsync(deliveryTag);
        }

        [Fact]
        public async Task AckCallAcknowledgementOnEachMessage()
        {
            var baseAck = Substitute.For<IAcknowledgement>();
            var ack = new EachMessageAcknowledger(baseAck);

            await ack.AckAsync(1);
            await ack.AckAsync(2);

            await baseAck.Received(2).AckAsync(Arg.Any<ulong>(), Arg.Any<bool>());
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(2, false)]
        [InlineData(12345, false)]
        public async Task NackPassParametersToBase(ulong deliveryTag, bool requeue)
        {
            var baseAck = Substitute.For<IAcknowledgement>();
            var ack = new EachMessageAcknowledger(baseAck);

            await ack.NackAsync(deliveryTag, requeue);

            await baseAck.Received().NackAsync(deliveryTag, requeue);
        }

        [Fact]
        public async Task NackCallAcknowledgementOnEachMessage()
        {
            var baseAck = Substitute.For<IAcknowledgement>();
            var ack = new EachMessageAcknowledger(baseAck);

            await ack.NackAsync(1, false);
            await ack.NackAsync(2, false);

            await baseAck.Received(2).NackAsync(Arg.Any<ulong>(), Arg.Any<bool>());
        }

        [Fact]
        public async Task CanDisposeTwice()
        {
            var baseAck = Substitute.For<IAcknowledgement>();
            var ack = new EachMessageAcknowledger(baseAck);

            await ack.DisposeAsync();

            var ex = await Record.ExceptionAsync(async () => await ack.DisposeAsync());

            Assert.Null(ex);
        }

        [Fact]
        public async Task ThrowsIfDisposed()
        {
            var baseAck = Substitute.For<IAcknowledgement>();
            var ack = new EachMessageAcknowledger(baseAck);

            await ack.DisposeAsync();

            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await ack.AckAsync(1));
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await ack.NackAsync(2, true));
        }
    }
}