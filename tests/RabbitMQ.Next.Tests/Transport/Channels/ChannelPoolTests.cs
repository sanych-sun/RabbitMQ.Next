using System;
using System.Collections.Generic;
using NSubstitute;
using RabbitMQ.Next.Transport.Channels;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Channels
{
    public class ChannelPoolTests
    {
        [Fact]
        public void ShouldStartWithZero()
        {
            var channelPool = new ChannelPool();
            var channel = Substitute.For<IChannelInternal>();

            var channelNumber = channelPool.Register(channel);

            Assert.Equal(0, channelNumber);
        }

        [Theory]
        [InlineData(5, 10)]
        [InlineData(15, 10)]
        public void ShouldAssignNext(int count, int initialPoolSize)
        {
            var channelPool = new ChannelPool(initialPoolSize);

            for (var i = 0; i < count; i++)
            {
                var channel = Substitute.For<IChannelInternal>();
                var number = channelPool.Register(channel);

                Assert.Equal(i, number);
            }
        }

        [Fact]
        public void CanGetByIndex()
        {
            var channelPool = new ChannelPool();

            var channel1 = Substitute.For<IChannelInternal>();
            var channel2 = Substitute.For<IChannelInternal>();
            var channel3 = Substitute.For<IChannelInternal>();

            var ch1 = channelPool.Register(channel1);
            var ch2 = channelPool.Register(channel2);
            var ch3 = channelPool.Register(channel3);

            Assert.Equal(channel1, channelPool[ch1]);
            Assert.Equal(channel2, channelPool[ch2]);
            Assert.Equal(channel3, channelPool[ch3]);
        }

        [Theory]
        [InlineData(10, 0, 1)]
        [InlineData(10, 5, 6)]
        [InlineData(10, 5, 9)]
        [InlineData(10, 5, 11)]
        [InlineData(10, 10, 11)]
        public void ThrowsIfOutOfRange(int initialSize, int count, int index)
        {
            var channelPool = new ChannelPool(initialSize);

            for (var i = 0; i < count; i++)
            {
                var channel = Substitute.For<IChannelInternal>();
                channelPool.Register(channel);
            }

            Assert.Throws<ArgumentOutOfRangeException>(() => channelPool[index]);
        }

        [Fact]
        public void ThrowsIfAccessToReleased()
        {
            var channelPool = new ChannelPool();

            var index1 = channelPool.Register(Substitute.For<IChannelInternal>());
            var index2 = channelPool.Register(Substitute.For<IChannelInternal>());

            channelPool.Release(index2);
            Assert.Throws<ArgumentOutOfRangeException>(() => channelPool[index2]);
        }

        [Fact]
        public void ShouldReuseReturnedNumbers()
        {
            var channelPool = new ChannelPool();

            for (var i = 0; i < 5; i++)
            {
                var channel = Substitute.For<IChannelInternal>();
                channelPool.Register(channel);
            }

            channelPool.Release(2);
            channelPool.Release(3);

            Assert.Equal(2, channelPool.Register(Substitute.For<IChannelInternal>()));
            Assert.Equal(3, channelPool.Register(Substitute.For<IChannelInternal>()));
            Assert.Equal(5, channelPool.Register(Substitute.For<IChannelInternal>()));
        }

        [Theory]
        [MemberData(nameof(ReleaseTestCases))]
        public void ReleaseAllShouldCompleteAll(Exception exception)
        {
            var channelPool = new ChannelPool();
            var channel1 = Substitute.For<IChannelInternal>();
            var channel2 = Substitute.For<IChannelInternal>();

            channelPool.Register(channel1);
            channelPool.Register(channel2);

            channelPool.ReleaseAll(exception);

            channel1.Received().SetCompleted(exception);
            channel2.Received().SetCompleted(exception);
        }

        public static IEnumerable<object[]> ReleaseTestCases()
        {
            yield return new object[] {null};
            yield return new object[] {new Exception()};
        }
    }
}