using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Tests.Mocks;
using RabbitMQ.Next.TopologyBuilder.Commands;
using RabbitMQ.Next.Transport.Methods.Exchange;
using Xunit;

namespace RabbitMQ.Next.Tests.TopologyBuilder;

public class ExchangeDeclareCommandTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ThrowsOnEmptyExchange(string exchange)
    {
        Assert.Throws<ArgumentNullException>(() => new ExchangeDeclareCommand(exchange, "type"));
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ThrowsOnEmptyType(string type)
    {
        Assert.Throws<ArgumentNullException>(() => new ExchangeDeclareCommand("exchange", type));
    }

    [Theory]
    [InlineData("exchange", "topic", false, false)]
    [InlineData("exchange", "topic", true, false)]
    [InlineData("exchange", "topic", false, true)]
    [InlineData("exchange", "topic", true, true, "arg1", "vl1", "arg2", "vl2")]
    [InlineData("exchange", "topic", true, true, "arg1", "vl1")]
    public async Task ExecuteCommandAsync(string exchange, string type, bool isInternal, bool autoDelete, params string[] arguments)
    {
        var channel = Substitute.For<IChannel>();
        var cmd = new ExchangeDeclareCommand(exchange, type);
        
        if (isInternal)
        {
            cmd.Internal();
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
                string.Equals(b.Exchange, exchange) 
                && string.Equals(b.Type, type)
                && b.Durable == true
                && b.Internal == isInternal
                && b.AutoDelete == autoDelete
                && b.Passive == false
                && Helpers.DictionaryEquals(b.Arguments, args)),
            Arg.Any<CancellationToken>());
    }
}