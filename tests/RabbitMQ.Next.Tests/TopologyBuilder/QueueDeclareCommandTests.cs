using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Tests.Mocks;
using RabbitMQ.Next.TopologyBuilder.Commands;
using RabbitMQ.Next.Transport.Methods.Queue;
using Xunit;

namespace RabbitMQ.Next.Tests.TopologyBuilder;

public class QueueDeclareCommandTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ThrowsOnEmptyQueue(string queue)
    {
        Assert.Throws<ArgumentNullException>(() => new QueueDeclareCommand(queue));
    }
    

    [Theory]
    [InlineData("queue", false, false)]
    [InlineData("queue", true, false)]
    [InlineData("queue", false, true)]
    [InlineData("queue", true, true, "arg1", "vl1")]
    [InlineData("queue", true, true, "arg1", "vl1", "arg2", "vl2")]
    public async Task ExecuteCommandAsync(string queue, bool exclusive, bool autoDelete, params string[] arguments)
    {
        var channel = Substitute.For<IChannel>();
        var cmd = new QueueDeclareCommand(queue);
        
        if (exclusive)
        {
            cmd.Exclusive();
        }

        if (autoDelete)
        {
            cmd.AutoDelete();
        }
        
        var args = arguments.ToArgsDictionary();

        if (args != null)
        {
            foreach (var arg in args)
            {
                cmd.Argument(arg.Key, arg.Value);
            }
        }

        await cmd.ExecuteAsync(channel);

        await channel.Received().SendAsync<DeclareMethod, DeclareOkMethod>(
            Arg.Is<DeclareMethod>(b => 
                string.Equals(b.Queue, queue)
                && b.Durable == true
                && b.Exclusive == exclusive
                && b.AutoDelete == autoDelete
                && b.Passive == false
                && Helpers.DictionaryEquals(b.Arguments, args)),
            Arg.Any<CancellationToken>());
    }
}