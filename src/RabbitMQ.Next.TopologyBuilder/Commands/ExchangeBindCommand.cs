using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Transport.Methods.Exchange;

namespace RabbitMQ.Next.TopologyBuilder.Commands;

internal class ExchangeBindCommand : IBindingDeclaration, ICommand
{
    private readonly string destination;
    private readonly string source;
    private readonly string routingKey;
    private Dictionary<string, object> arguments;

    public ExchangeBindCommand(string destination, string source, string routingKey)
    {
        if (string.IsNullOrEmpty(destination))
        {
            throw new ArgumentNullException(nameof(destination));
        }

        if (string.IsNullOrEmpty(source))
        {
            throw new ArgumentNullException(nameof(source));
        }
        
        this.destination = destination;
        this.source = source;
        this.routingKey = routingKey;
    }

    public IBindingDeclaration Argument(string key, object value)
    {
        this.arguments ??= new Dictionary<string, object>();
        this.arguments[key] = value;

        return this;
    }

    public Task ExecuteAsync(IChannel channel, CancellationToken cancellation = default)
        => channel.SendAsync<BindMethod, BindOkMethod>(
            new BindMethod(this.destination, this.source, this.routingKey, this.arguments), cancellation);
}