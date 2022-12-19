using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Transport.Methods.Exchange;

namespace RabbitMQ.Next.TopologyBuilder.Commands;

internal class ExchangeDeleteCommand : IExchangeDeletion, ICommand
{
    private readonly string exchange;
    private bool cancelBindings;

    public ExchangeDeleteCommand(string exchange)
    {
        if (string.IsNullOrEmpty(exchange))
        {
            throw new ArgumentNullException(nameof(exchange));
        }
        
        this.exchange = exchange;
    }

    public IExchangeDeletion CancelBindings()
    {
        this.cancelBindings = true;

        return this;
    }

    public Task ExecuteAsync(IChannel channel, CancellationToken cancellation = default)
        => channel.SendAsync<DeleteMethod, DeleteOkMethod>(new DeleteMethod(this.exchange, !this.cancelBindings), cancellation);
}