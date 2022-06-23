using System;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Channels;
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
                Arg.Is<AckMethod>(m => m.Multiple == false && m.DeliveryTag == 142));
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
        public async Task CanAckAndNack()
        {
            var channel = Substitute.For<IChannel>();
            var acknowledgement = new DefaultAcknowledgement(channel);

            await acknowledgement.AckAsync(1);
            await acknowledgement.AckAsync(2);
            await acknowledgement.NackAsync(3, true);
            await acknowledgement.NackAsync(4, false);
            await acknowledgement.AckAsync(5);
            
            await Task.Delay(50);
            
            await channel.Received().SendAsync(
                Arg.Is<AckMethod>(m => m.Multiple == false && m.DeliveryTag == 1));
            await channel.Received().SendAsync(
                Arg.Is<AckMethod>(m => m.Multiple == false && m.DeliveryTag == 2));
            await channel.Received().SendAsync(
                Arg.Is<NackMethod>(m => m.Multiple == false && m.DeliveryTag == 3 && m.Requeue ));
            await channel.Received().SendAsync(
                Arg.Is<NackMethod>(m => m.Multiple == false && m.DeliveryTag == 4 && !m.Requeue));
            await channel.Received().SendAsync(
                Arg.Is<AckMethod>(m => m.Multiple == false && m.DeliveryTag == 5));
            
        }

        [Fact]
        public async Task CanDisposeMultiple()
        {
            var channel = Substitute.For<IChannel>();
            var acknowledgement = new DefaultAcknowledgement(channel);

            await acknowledgement.DisposeAsync();

            var record = await Record.ExceptionAsync(async () => await acknowledgement.DisposeAsync());
            Assert.Null(record);
        }
    }
}