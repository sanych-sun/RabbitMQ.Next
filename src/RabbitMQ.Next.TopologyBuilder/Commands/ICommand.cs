using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;

namespace RabbitMQ.Next.TopologyBuilder.Commands;

internal interface ICommand
{
    Task ExecuteAsync(IChannel channel, CancellationToken cancellation = default);
}

internal interface ICommand<TResult>
{
    Task<TResult> ExecuteAsync(IChannel channel, CancellationToken cancellation = default);
}