using System;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Consumer;
using RabbitMQ.Next.Transport.Methods.Basic;
using Xunit;

namespace RabbitMQ.Next.Tests.Consumer
{
    public class DefaultAcknowledgementTests
    {
        [Fact]
        public void ThrowsOnNullChannel()
        {
            Assert.Throws<ArgumentNullException>(() => new DefaultAcknowledgement(null));
        }

        [Fact]
        public async Task CanAck()
        {
            var channel = Substitute.For<IChannel>();
            var acknowledgement = new DefaultAcknowledgement(channel);

            await acknowledgement.AckAsync(142);

            await Task.Delay(10);

            await channel.Received().SendAsync(
                Arg.Is<AckMethod>(m => m.Multiple == true && m.DeliveryTag == 142));
        }

        [Theory]
        [InlineData(142, false)]
        [InlineData(42, true)]
        public async Task CanNack(ulong deliveryTag, bool requeue)
        {
            var channel = Substitute.For<IChannel>();
            var acknowledgement = new DefaultAcknowledgement(channel);

            await acknowledgement.NackAsync(deliveryTag, requeue);

            await Task.Delay(10);

            await channel.Received().SendAsync(
                Arg.Is<NackMethod>(m => m.Multiple == false && m.DeliveryTag == deliveryTag && m.Requeue == requeue));
        }

        [Fact]
        public async Task CanAckMultiple()
        {
            var tcs = new TaskCompletionSource();
            var channel = Substitute.For<IChannel>();
            channel.SendAsync(Arg.Any<AckMethod>()).Returns(new ValueTask(tcs.Task));
            var acknowledgement = new DefaultAcknowledgement(channel);

            await acknowledgement.AckAsync(1);
            await Task.Delay(50);

            await acknowledgement.AckAsync(2);
            await acknowledgement.AckAsync(3);
            await acknowledgement.AckAsync(4);
            await acknowledgement.AckAsync(5);

            tcs.SetResult();
            await Task.Delay(50);

            Received.InOrder(async () => {
                await channel.Received().SendAsync(
                    Arg.Is<AckMethod>(m => m.Multiple == true && m.DeliveryTag == 1));
                await channel.Received().SendAsync(
                    Arg.Is<AckMethod>(m => m.Multiple == true && m.DeliveryTag == 5));
            });
        }

        [Fact]
        public async Task ShouldNackAtFirstOnMultiple()
        {
            var tcs = new TaskCompletionSource();
            var channel = Substitute.For<IChannel>();
            channel.SendAsync(Arg.Any<AckMethod>()).Returns(new ValueTask(tcs.Task));
            var acknowledgement = new DefaultAcknowledgement(channel);

            await acknowledgement.AckAsync(1);
            await Task.Delay(50);

            await acknowledgement.AckAsync(2);
            await acknowledgement.NackAsync(3, false);
            await acknowledgement.AckAsync(4);
            await acknowledgement.AckAsync(5);

            tcs.SetResult();


            Received.InOrder(async () => {
                await channel.Received().SendAsync(
                    Arg.Is<AckMethod>(m => m.Multiple == true && m.DeliveryTag == 1));
                await channel.Received().SendAsync(
                    Arg.Is<NackMethod>(m => m.Multiple == false && m.DeliveryTag == 3 && m.Requeue == false));
                await channel.Received().SendAsync(
                    Arg.Is<AckMethod>(m => m.Multiple == true && m.DeliveryTag == 5));
            });
        }

        [Fact]
        public async Task AskThrowsIfDisposed()
        {
            var channel = Substitute.For<IChannel>();
            var acknowledgement = new DefaultAcknowledgement(channel);

            await acknowledgement.DisposeAsync();

            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await acknowledgement.AckAsync(22));
        }

        [Fact]
        public async Task NaskThrowsIfDisposed()
        {
            var channel = Substitute.For<IChannel>();
            var acknowledgement = new DefaultAcknowledgement(channel);

            await acknowledgement.DisposeAsync();

            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await acknowledgement.NackAsync(22, false));
        }
    }
}