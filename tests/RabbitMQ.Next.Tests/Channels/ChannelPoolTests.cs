using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Channels;
using Xunit;

namespace RabbitMQ.Next.Tests.Channels;

public class ChannelPoolTests
{
    [Fact]
    public void PoolStartsWithOne()
    {

/* Unmerged change from project 'RabbitMQ.Next.Tests'
Before:
        var factory = this.MockFactory();
After:
        var factory = ChannelPoolTests.MockFactory();
*/
        var factory = MockFactory();
        var pool = new ChannelPool(factory);

        pool.Create();

        factory.Received().Invoke(1);
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(5, 22)]
    public void CanResize(int initialSize, int num)
    {

/* Unmerged change from project 'RabbitMQ.Next.Tests'
Before:
        var factory = this.MockFactory();
After:
        var factory = ChannelPoolTests.MockFactory();
*/
        var factory = MockFactory();
        var pool = new ChannelPool(factory, initialSize);


        var record = Record.Exception(() =>
        {
            for (var i = 0; i < num; i++)
            {
                pool.Create();
            }
        });
        
        Assert.Null(record);
    }

    [Theory]
    [InlineData(10, 1)]
    [InlineData(100, 10)]
    public async Task CreateTests(int number, int concurrencyLevel)
    {

/* Unmerged change from project 'RabbitMQ.Next.Tests'
Before:
        var factory = this.MockFactory();
After:
        var factory = ChannelPoolTests.MockFactory();
*/
        var factory = MockFactory();
        var pool = new ChannelPool(factory);

        var createTasks = Enumerable.Range(0, number)
            .Select(async _ =>
            {
                await Task.Yield();
                for (var i = 0; i < number; i++)
                {
                    pool.Create();
                }
            })
            .ToArray();

        await Task.WhenAll(createTasks);

        factory.DidNotReceive().Invoke(0);
        for (ushort i = 1; i < number * concurrencyLevel; i++)
        {
            factory.Received().Invoke(i);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    public void CanGet(int num)
    {

/* Unmerged change from project 'RabbitMQ.Next.Tests'
Before:
        var factory = this.MockFactory();
After:
        var factory = ChannelPoolTests.MockFactory();
*/
        var factory = MockFactory();
        var pool = new ChannelPool(factory);
        var channels = new Dictionary<int, IChannel>();

        for (var i = 1; i < num; i++)
        {
            channels.Add(i, pool.Create());
        }

        foreach(var (key,channel) in channels)
        {
            var ch = pool.Get((ushort)key);
            Assert.Equal(channel, ch);
        }
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(10, 0)]
    [InlineData(10, 11)]
    [InlineData(10, 100)]
    public void GetThrowsOnUnknownChannel(int channels, ushort ch)
    {

/* Unmerged change from project 'RabbitMQ.Next.Tests'
Before:
        var factory = this.MockFactory();
After:
        var factory = ChannelPoolTests.MockFactory();
*/
        var factory = MockFactory();
        var pool = new ChannelPool(factory);

        for (var i = 0; i < channels; i++)
        {
            pool.Create();
        }

        Assert.Throws<ArgumentOutOfRangeException>(() => pool.Get(ch));
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(10, 5)]
    [InlineData(10, 5, 2)]
    public async Task CanReuseChannelAfterCompletion(int channels, params int[] chToComplete)
    {

/* Unmerged change from project 'RabbitMQ.Next.Tests'
Before:
        var factory = this.MockFactory();
After:
        var factory = ChannelPoolTests.MockFactory();
*/
        var factory = MockFactory();
        var pool = new ChannelPool(factory);

        for (var i = 0; i < channels; i++)
        {
            pool.Create();
        }

        foreach (var i in chToComplete)
        {
            var ch = pool.Get((ushort)i);
            ch.TryComplete();
        }

        await Task.Delay(10);

        factory.ClearReceivedCalls();
        for (var i = 0; i < chToComplete.Length; i++)
        {
            pool.Create();
        }

        foreach (var ch in chToComplete)
        {
            factory.Received(1)((ushort)ch);
        }
    }

    [Fact]
    public async Task CanReleaseAll()
    {

/* Unmerged change from project 'RabbitMQ.Next.Tests'
Before:
        var factory = this.MockFactory();
After:
        var factory = ChannelPoolTests.MockFactory();
*/
        var factory = MockFactory();
        var pool = new ChannelPool(factory);

        for (var i = 0; i < 10; i++)
        {
            pool.Create();
        }

        pool.ReleaseAll();
        await Task.Delay(10);

        factory.ClearReceivedCalls();
        pool.Create();

        factory.Received(1)(Arg.Is<ushort>(u => u > 0 && u <= 10));
    }

    private static Func<ushort, IChannelInternal> MockFactory()
    {
        var factory = Substitute.For<Func<ushort, IChannelInternal>>();
        factory(Arg.Any<ushort>()).Returns(args =>
        {
            var t = new TaskCompletionSource();
            var ch = Substitute.For<IChannelInternal>();
            ch.Completion.Returns(t.Task);
            ch.TryComplete(Arg.Any<Exception>()).Returns(args =>
            {
                var ex = (Exception)args[0];
                if (ex == null)
                {
                    t.SetResult();
                }
                else
                {
                    t.SetException(ex);
                }
                return true;
            });
            return ch;
        });
        return factory;
    }
}
