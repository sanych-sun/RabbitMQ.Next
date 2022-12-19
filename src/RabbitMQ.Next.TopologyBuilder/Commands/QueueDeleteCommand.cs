using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Transport.Methods.Queue;

namespace RabbitMQ.Next.TopologyBuilder.Commands;

internal class QueueDeleteCommand : IQueueDeletion, ICommand
{
    private readonly string queue;
    private bool cancelConsumers;
    private bool discardMessages;

    public QueueDeleteCommand(string queue)
    {
        if (string.IsNullOrEmpty(queue))
        {
            throw new ArgumentNullException(nameof(queue));
        }

        this.queue = queue;
    }

    public IQueueDeletion CancelConsumers()
    {
        this.cancelConsumers = true;
        return this;
    }

    public IQueueDeletion DiscardMessages()
    {
        this.discardMessages = true;
        return this;
    }

    public Task ExecuteAsync(IChannel channel, CancellationToken cancellation = default)
        => channel.SendAsync<DeleteMethod, DeleteOkMethod>(
            new DeleteMethod(this.queue, !this.cancelConsumers, !this.discardMessages), cancellation);
}