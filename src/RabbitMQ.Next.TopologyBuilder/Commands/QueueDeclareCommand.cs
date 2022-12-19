using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Transport.Methods.Queue;

namespace RabbitMQ.Next.TopologyBuilder.Commands;

internal class QueueDeclareCommand : IClassicQueueDeclaration, IQuorumQueueDeclaration, IStreamQueueDeclaration, ICommand
{
    private readonly string queue;
    private bool exclusive;
    private bool autoDelete;
    private Dictionary<string, object> arguments;
    
    public QueueDeclareCommand(string queue)
    {
        if (string.IsNullOrEmpty(queue))
        {
            throw new ArgumentNullException(nameof(queue));
        }
        
        this.queue = queue;
    }


    public QueueDeclareCommand Argument(string key, object value)
    {
        this.arguments ??= new Dictionary<string, object>();
        this.arguments[key] = value;
        return this;
    }

    public QueueDeclareCommand AutoDelete()
    {
        this.autoDelete = true;
        return this;
    }
    
    public QueueDeclareCommand Exclusive()
    {
        this.exclusive = true;
        return this;
    }

    public Task ExecuteAsync(IChannel channel, CancellationToken cancellation = default)
        => channel.SendAsync<DeclareMethod, DeclareOkMethod>(
            new DeclareMethod(this.queue, true, this.exclusive, this.autoDelete, this.arguments), cancellation);

    #region IClassicQueueDeclaration
    
    IClassicQueueDeclaration IClassicQueueDeclaration.Argument(string key, object value) 
        => this.Argument(key, value);

    IClassicQueueDeclaration IClassicQueueDeclaration.Exclusive()
        => this.Exclusive();

    IClassicQueueDeclaration IClassicQueueDeclaration.AutoDelete()
        => this.AutoDelete();

    #endregion

    #region IQuorumQueueDeclaration

    IQuorumQueueDeclaration IQuorumQueueDeclaration.Argument(string key, object value)
        => this.Argument(key, value);

    IQuorumQueueDeclaration IQuorumQueueDeclaration.AutoDelete()
        => this.AutoDelete();

    #endregion
    
    #region IStreamQueueDeclaration
    IStreamQueueDeclaration IStreamQueueDeclaration.Argument(string key, object value)
        => this.Argument(key, value);
    
    #endregion
}