using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Exceptions;
using RabbitMQ.Next.Transport.Methods.Exchange;

namespace RabbitMQ.Next.TopologyBuilder.Commands;

internal class ExchangeExistsCommand : ICommand<bool>
{
    private readonly string exchange;

    public ExchangeExistsCommand(string exchange)
    {
        if (string.IsNullOrEmpty(exchange))
        {
            throw new ArgumentNullException(nameof(exchange));
        }
        
        this.exchange = exchange;
    }
    
    public async Task<bool> ExecuteAsync(IChannel channel, CancellationToken cancellation = default)
    {
        try
        {
            await channel.SendAsync<DeclareMethod, DeclareOkMethod>(new DeclareMethod(this.exchange), cancellation);
            return true;
        }
        catch(ChannelException ex) when (ex.FailedMethodId == MethodId.ExchangeDeclare)
        {
            return false;
        }
    }
}