using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Transport.Methods.Queue;

namespace RabbitMQ.Next.TopologyBuilder.Commands;

internal class QueuePurgeCommand : ICommand
{
    private readonly string queue;

    public QueuePurgeCommand(string queue)
    {
        if (string.IsNullOrEmpty(queue))
        {
            throw new ArgumentNullException(nameof(queue));
        }
        
        this.queue = queue;
    }

    public Task ExecuteAsync(IChannel channel, CancellationToken cancellation = default)
        => channel.SendAsync<PurgeMethod, PurgeOkMethod>(new PurgeMethod(this.queue), cancellation);
}