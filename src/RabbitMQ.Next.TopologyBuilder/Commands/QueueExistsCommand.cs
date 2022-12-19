using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Exceptions;
using RabbitMQ.Next.Transport.Methods.Queue;

namespace RabbitMQ.Next.TopologyBuilder.Commands;

internal class QueueExistsCommand : ICommand<bool>
{
    private readonly string queue;

    public QueueExistsCommand(string queue)
    {
        if (string.IsNullOrEmpty(queue))
        {
            throw new ArgumentNullException(nameof(queue));
        }
        
        this.queue = queue;
    }
    
    public async Task<bool> ExecuteAsync(IChannel channel, CancellationToken cancellation = default)
    {
        try
        {
            await channel.SendAsync<DeclareMethod, DeclareOkMethod>(new DeclareMethod(this.queue), cancellation);
            return true;
        }
        catch(ChannelException ex) when (ex.FailedMethodId == MethodId.QueueDeclare)
        {
            return false;
        }
    }
}