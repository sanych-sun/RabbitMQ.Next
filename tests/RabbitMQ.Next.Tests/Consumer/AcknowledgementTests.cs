using System;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Consumer;
using RabbitMQ.Next.Transport.Methods.Basic;
using Xunit;

namespace RabbitMQ.Next.Tests.Consumer
{
    public class AcknowledgementTests
    {
        [Theory]
        [InlineData(123, false)]
        [InlineData(321, true)]
        public async Task AckTests(ulong deliveryTag, bool isMultiple)
        {
            var channel = Substitute.For<IChannel>();
            var acknowledgement = new Acknowledgement(channel);

            await acknowledgement.AckAsync(deliveryTag, isMultiple);

            await channel.Received().SendAsync(
                Arg.Is<AckMethod>(m => m.DeliveryTag == deliveryTag && m.Multiple == isMultiple));
        }

        [Theory]
        [InlineData(123, false)]
        [InlineData(321, true)]
        public async Task NackTests(ulong deliveryTag, bool requeue)
        {
            var channel = Substitute.For<IChannel>();
            var acknowledgement = new Acknowledgement(channel);

            await acknowledgement.NackAsync(deliveryTag, requeue);

            await channel.Received().SendAsync(
                Arg.Is<NackMethod>(m => m.DeliveryTag == deliveryTag && m.Requeue == requeue && m.Multiple == false));
        }

        [Fact]
        public async Task ThrowsIfDisposed()
        {
            var channel = Substitute.For<IChannel>();
            var acknowledgement = new Acknowledgement(channel);
            await acknowledgement.DisposeAsync();

            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await acknowledgement.AckAsync(123));
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await acknowledgement.NackAsync(123, false));
        }
    }
}