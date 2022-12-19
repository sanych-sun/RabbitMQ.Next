using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Transport.Methods.Exchange;

namespace RabbitMQ.Next.TopologyBuilder.Commands;

internal class ExchangeDeclareCommand : IExchangeDeclaration, ICommand
{
    private readonly string exchange;
    private readonly string type;
    private bool isInternal;
    private bool isAutoDelete;
    private Dictionary<string, object> arguments;

    public ExchangeDeclareCommand(string exchange, string type)
    {
        if (string.IsNullOrEmpty(exchange))
        {
            throw new ArgumentNullException(nameof(this.exchange));
        }

        if (string.IsNullOrEmpty(type))
        {
            throw new ArgumentNullException(nameof(type));
        }
        
        this.exchange = exchange;
        this.type = type;
    }

    public IExchangeDeclaration Internal()
    {
        this.isInternal = true;
        return this;
    }

    public IExchangeDeclaration AutoDelete()
    {
        this.isAutoDelete = true;
        return this;
    }


    public IExchangeDeclaration Argument(string key, object value)
    {
        this.arguments ??= new Dictionary<string, object>();
        this.arguments[key] = value;

        return this;
    }

    public Task ExecuteAsync(IChannel channel, CancellationToken cancellation = default)
        => channel.SendAsync<DeclareMethod, DeclareOkMethod>(
            new DeclareMethod(this.exchange, this.type, true, this.isAutoDelete, this.isInternal, this.arguments), cancellation);
}