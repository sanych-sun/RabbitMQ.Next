using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Consumer.Abstractions.Acknowledgement;
using Xunit;

namespace RabbitMQ.Next.Tests.Consumer.Acknowledgements
{
    public class MultipleMessageAcknowledgerTests
    {
        [Theory]
        [MemberData(nameof(WrongCtorParamsTestCases))]
        public void ThrowsOnWrongCtorParams(IAcknowledgement ack, TimeSpan timeout, int count)
        {
            Assert.ThrowsAny<ArgumentException>(() => new MultipleMessageAcknowledger(ack, timeout, count));
        }

        [Theory]
        [InlineData(5, 5, 1)]
        [InlineData(5, 10, 2)]
        [InlineData(7, 77, 11)]
        public async Task AckCallAcknowledgementOnCount(int limit, int messages, int expectedCalls)
        {
            var acknowledgement = Substitute.For<IAcknowledgement>();
            var acknowledger = new MultipleMessageAcknowledger(acknowledgement, TimeSpan.FromSeconds(100), limit);

            for (var i = 1; i <= messages; i++)
            {
                await acknowledger.AckAsync((ulong)i);
            }

            await acknowledgement.Received(expectedCalls).AckAsync(Arg.Any<ulong>(), true);
        }

        [Fact]
        public async Task AckCallAcknowledgementByTimeout()
        {
            var acknowledgement = Substitute.For<IAcknowledgement>();
            var acknowledger = new MultipleMessageAcknowledger(acknowledgement, TimeSpan.FromSeconds(1), 100);

            await acknowledger.AckAsync(1);
            await Task.Delay(TimeSpan.FromSeconds(2));

            await acknowledgement.Received().AckAsync(Arg.Any<ulong>(), true);
        }

        [Fact]
        public async Task AckNotCallEmptyAcknowledgement()
        {
            var acknowledgement = Substitute.For<IAcknowledgement>();
            var acknowledger = new MultipleMessageAcknowledger(acknowledgement, TimeSpan.FromSeconds(1), 100);

            await Task.Delay(TimeSpan.FromSeconds(2));

            await acknowledgement.DidNotReceive().AckAsync(Arg.Any<ulong>(), true);
        }

        [Theory]
        [InlineData(5, 1, 1)]
        [InlineData(5, 5, 1)]
        [InlineData(5, 6, 2)]
        [InlineData(5, 15, 3)]
        public async Task AckMakesAppropriateCallsAcknowledgement(int limit, int messages, int expectedCalls)
        {
            var acknowledgement = Substitute.For<IAcknowledgement>();
            var acknowledger = new MultipleMessageAcknowledger(acknowledgement, TimeSpan.FromSeconds(1), limit);

            for (var i = 1; i <= messages; i++)
            {
                await acknowledger.AckAsync((ulong)i);
            }

            // wait for ack by timeout
            await Task.Delay(TimeSpan.FromSeconds(2));

            await acknowledgement.Received(expectedCalls).AckAsync(Arg.Any<ulong>(), true);
        }

        [Theory]
        [InlineData(7, false)]
        [InlineData(42, true)]
        public async Task NackCallAcknowledgement(ulong deliveryTag, bool requeue)
        {
            var acknowledgement = Substitute.For<IAcknowledgement>();
            var acknowledger = new MultipleMessageAcknowledger(acknowledgement, TimeSpan.FromSeconds(100), 100);

            await acknowledger.NackAsync(deliveryTag, requeue);

            await acknowledgement.Received().NackAsync(deliveryTag, requeue);
        }

        [Fact]
        public async Task NackSendsPendingAcknowledgements()
        {
            var acknowledgement = Substitute.For<IAcknowledgement>();
            var acknowledger = new MultipleMessageAcknowledger(acknowledgement, TimeSpan.FromSeconds(100), 100);

            await acknowledger.AckAsync(1);
            await acknowledger.AckAsync(2);
            await acknowledger.NackAsync(3, false);

            await acknowledgement.Received().AckAsync(2, true);
            await acknowledgement.Received().NackAsync(3, false);
        }

        [Fact]
        public async Task CanDisposeTwice()
        {
            var acknowledgement = Substitute.For<IAcknowledgement>();
            var ack = new MultipleMessageAcknowledger(acknowledgement, TimeSpan.FromSeconds(5), 100);

            await ack.DisposeAsync();

            var ex = await Record.ExceptionAsync(async () => await ack.DisposeAsync());

            Assert.Null(ex);
        }

        [Fact]
        public async Task DisposeSendsPendingAcks()
        {
            var acknowledgement = Substitute.For<IAcknowledgement>();
            var ack = new MultipleMessageAcknowledger(acknowledgement, TimeSpan.FromSeconds(5), 100);

            await ack.AckAsync(1);
            await ack.AckAsync(2);
            
            await ack.DisposeAsync();
            
            await acknowledgement.Received().AckAsync(2, true);
        }
        
        [Fact]
        public async Task ThrowsIfDisposed()
        {
            var acknowledgement = Substitute.For<IAcknowledgement>();
            var ack = new MultipleMessageAcknowledger(acknowledgement, TimeSpan.FromSeconds(5), 100);

            await ack.DisposeAsync();

            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await ack.AckAsync(1));
            await Assert.ThrowsAsync<ObjectDisposedException>(async () => await ack.NackAsync(2, true));
        }

        public static IEnumerable<object[]> WrongCtorParamsTestCases()
        {
            yield return new object[] {null, TimeSpan.FromSeconds(5), 10};

            yield return new object[] {Substitute.For<IAcknowledgement>(), TimeSpan.Zero, 10};

            yield return new object[] {Substitute.For<IAcknowledgement>(), TimeSpan.FromSeconds(-5), 10};

            yield return new object[] {Substitute.For<IAcknowledgement>(), TimeSpan.FromSeconds(5), 0};

            yield return new object[] {Substitute.For<IAcknowledgement>(), TimeSpan.FromSeconds(5), -10};
        }
    }
}