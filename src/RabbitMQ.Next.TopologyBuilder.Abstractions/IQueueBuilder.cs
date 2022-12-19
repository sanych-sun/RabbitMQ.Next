using System;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.TopologyBuilder;

public interface IQueueBuilder
{
    Task<bool> ExistsAsync(string queue, CancellationToken cancellation = default);
    
    Task DeclareQuorumAsync(string queue, Action<IQuorumQueueDeclaration> builder = null, CancellationToken cancellation = default);
    
    Task DeclareClassicAsync(string queue, Action<IClassicQueueDeclaration> builder = null, CancellationToken cancellation = default);

    Task DeclareStreamAsync(string queue, Action<IStreamQueueDeclaration> builder = null, CancellationToken cancellation = default);

    Task DeleteAsync(string queue, Action<IQueueDeletion> builder = null, CancellationToken cancellation = default);

    Task PurgeAsync(string queue, CancellationToken cancellation = default);

    Task BindAsync(string queue, string exchange, string routingKey = null, Action<IBindingDeclaration> builder = null, CancellationToken cancellation = default);

    Task UnbindAsync(string queue, string exchange, string routingKey = null, Action<IBindingDeclaration> builder = null, CancellationToken cancellation = default);
}
