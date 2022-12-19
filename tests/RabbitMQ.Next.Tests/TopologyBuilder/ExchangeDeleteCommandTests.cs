using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.TopologyBuilder.Commands;
using RabbitMQ.Next.Transport.Methods.Exchange;
using Xunit;

namespace RabbitMQ.Next.Tests.TopologyBuilder;

public class ExchangeDeleteCommandTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ThrowsOnEmptyExchange(string exchange)
    {
        Assert.Throws<ArgumentNullException>(() => new ExchangeDeleteCommand(exchange));
    }
    

    [Theory]
    [InlineData("exchange", false)]
    [InlineData("exchange", true)]
    public async Task ExecuteCommandAsync(string exchange, bool cancelBindings)
    {
        var channel = Substitute.For<IChannel>();
        var cmd = new ExchangeDeleteCommand(exchange);
        
        if (cancelBindings)
        {
            cmd.CancelBindings();
        }
        
        await cmd.ExecuteAsync(channel);

        await channel.Received().SendAsync<DeleteMethod, DeleteOkMethod>(
            Arg.Is<DeleteMethod>(b => 
                string.Equals(b.Exchange, exchange) 
                && b.UnusedOnly == !cancelBindings),
            Arg.Any<CancellationToken>());
    }
}