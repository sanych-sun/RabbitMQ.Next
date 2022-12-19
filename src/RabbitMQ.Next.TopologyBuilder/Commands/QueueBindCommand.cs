using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Transport.Methods.Queue;

namespace RabbitMQ.Next.TopologyBuilder.Commands;

internal class QueueBindCommand : IBindingDeclaration, ICommand
{
    private readonly string queue;
    private readonly string exchange;
    private readonly string routingKey;
    private Dictionary<string, object> arguments;

    public QueueBindCommand(string queue, string exchange, string routingKey)
    {
        if (string.IsNullOrEmpty(queue))
        {
            throw new ArgumentNullException(nameof(queue));
        }
        
        if (string.IsNullOrEmpty(exchange))
        {
            throw new ArgumentNullException(nameof(exchange));
        }
        
        this.queue = queue;
        this.exchange = exchange;
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
        new BindMethod(this.queue, this.exchange, this.routingKey, this.arguments), cancellation);
}