using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.TopologyBuilder.Commands;

namespace RabbitMQ.Next.TopologyBuilder;

internal class TopologyBuilder : ITopologyBuilder, IExchangeBuilder, IQueueBuilder, IDisposable
{
    private readonly IConnection connection;
    private bool disposed; 

    public TopologyBuilder(IConnection connection)
    {
        this.connection = connection;
    }
    
    public void Dispose()
    {
        this.disposed = true;
    }

    public IExchangeBuilder Exchange => this;

    public IQueueBuilder Queue => this;
    
    #region IExchangeBuilder
    
    Task<bool> IExchangeBuilder.ExistsAsync(string exchange, CancellationToken cancellation)
    {
        var cmd = new ExchangeExistsCommand(exchange);
        return this.ExecuteCommand(cmd, cancellation);
    }

    Task IExchangeBuilder.DeclareAsync(string exchange, string type, Action<IExchangeDeclaration> builder, CancellationToken cancellation)
    {
        var cmd = new ExchangeDeclareCommand(exchange, type);
        builder?.Invoke(cmd);
        return this.ExecuteCommand(cmd, cancellation);
    }

    Task IExchangeBuilder.DeleteAsync(string exchange, Action<IExchangeDeletion> builder, CancellationToken cancellation)
    {
        var cmd = new ExchangeDeleteCommand(exchange);
        builder?.Invoke(cmd);
        return this.ExecuteCommand(cmd, cancellation);
    }

    Task IExchangeBuilder.BindAsync(string destination, string source, string routingKey, Action<IBindingDeclaration> builder, CancellationToken cancellation)
    {
        var cmd = new ExchangeBindCommand(destination, source, routingKey);
        builder?.Invoke(cmd);
        return this.ExecuteCommand(cmd, cancellation);
    }

    Task IExchangeBuilder.UnbindAsync(string destination, string source, string routingKey, Action<IBindingDeclaration> builder, CancellationToken cancellation)
    {
        var cmd = new ExchangeUnbindCommand(destination, source, routingKey);
        builder?.Invoke(cmd);
        return this.ExecuteCommand(cmd, cancellation);
    }

    #endregion
    
    #region IQueueBuilder
    
    Task IQueueBuilder.DeclareQuorumAsync(string queue, Action<IQuorumQueueDeclaration> builder, CancellationToken cancellation)
    {
        var cmd = new QueueDeclareCommand(queue);
        cmd.Argument("x-queue-type", "quorum");
        builder?.Invoke(cmd);

        return this.ExecuteCommand(cmd, cancellation);
    }

    Task IQueueBuilder.DeclareClassicAsync(string queue, Action<IClassicQueueDeclaration> builder, CancellationToken cancellation)
    {
        var cmd = new QueueDeclareCommand(queue);
        builder?.Invoke(cmd);

        return this.ExecuteCommand(cmd, cancellation);
    }

    Task IQueueBuilder.DeclareStreamAsync(string queue, Action<IStreamQueueDeclaration> builder, CancellationToken cancellation)
    {
        var cmd = new QueueDeclareCommand(queue);
        cmd.Argument("x-queue-type", "stream");
        builder?.Invoke(cmd);

        return this.ExecuteCommand(cmd, cancellation);
    }

    Task<bool> IQueueBuilder.ExistsAsync(string queue, CancellationToken cancellation)
    {
        var cmd = new QueueExistsCommand(queue);
        return this.ExecuteCommand(cmd, cancellation);
    }

    Task IQueueBuilder.DeleteAsync(string queue, Action<IQueueDeletion> builder, CancellationToken cancellation)
    {
        var cmd = new QueueDeleteCommand(queue);
        builder?.Invoke(cmd);
        return this.ExecuteCommand(cmd, cancellation);
    }

    Task IQueueBuilder.PurgeAsync(string queue, CancellationToken cancellation)
    {
        var cmd = new QueuePurgeCommand(queue);
        return this.ExecuteCommand(cmd, cancellation);
    }

    Task IQueueBuilder.BindAsync(string queue, string exchange, string routingKey, Action<IBindingDeclaration> builder, CancellationToken cancellation)
    {
        var cmd = new QueueBindCommand(queue, exchange, routingKey);
        builder?.Invoke(cmd);
        return this.ExecuteCommand(cmd, cancellation);
    }

    Task IQueueBuilder.UnbindAsync(string queue, string exchange, string routingKey, Action<IBindingDeclaration> builder, CancellationToken cancellation)
    {
        var cmd = new QueueUnbindCommand(queue, exchange, routingKey);
        builder?.Invoke(cmd);
        return this.ExecuteCommand(cmd, cancellation);
    }

    #endregion

    private Task ExecuteCommand(ICommand cmd, CancellationToken cancellation)
    {
        if (this.disposed)
        {
            throw new ObjectDisposedException(nameof(TopologyBuilder));
        }
            
        return this.connection.UseChannelAsync((cmd, cancellation), (state, ch) => state.cmd.ExecuteAsync(ch, state.cancellation));
    }

    private Task<TResult> ExecuteCommand<TResult>(ICommand<TResult> cmd, CancellationToken cancellation)
    {
        if (this.disposed)
        {
            throw new ObjectDisposedException(nameof(TopologyBuilder));
        }
        
        return this.connection.UseChannelAsync((cmd, cancellation), (state, ch) => state.cmd.ExecuteAsync(ch, state.cancellation));
    }
}