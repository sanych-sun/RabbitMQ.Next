using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.TopologyBuilder.Commands;
using RabbitMQ.Next.Transport.Methods.Exchange;
using Xunit;

namespace RabbitMQ.Next.Tests.TopologyBuilder;

public class ExchangeExistsCommandTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ThrowsOnEmptyExchange(string exchange)
    {
        Assert.Throws<ArgumentNullException>(() => new ExchangeExistsCommand(exchange));
    }
    

    [Theory]
    [InlineData("exchange")]
    public async Task ExecuteCommandAsync(string exchange)
    {
        var channel = Substitute.For<IChannel>();
        var cmd = new ExchangeExistsCommand(exchange);
        
        
        await cmd.ExecuteAsync(channel);

        await channel.Received().SendAsync<DeclareMethod, DeclareOkMethod>(
            Arg.Is<DeclareMethod>(b => 
                        string.Equals(b.Exchange, exchange) 
                        && string.IsNullOrEmpty(b.Type)
                        && b.Internal == false
                        && b.AutoDelete == false
                        && b.Passive == true
                        && b.Arguments == null),
            Arg.Any<CancellationToken>());
    }
}