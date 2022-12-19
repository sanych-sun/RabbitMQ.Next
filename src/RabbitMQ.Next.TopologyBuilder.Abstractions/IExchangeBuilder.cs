using System;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.TopologyBuilder;

public interface IExchangeBuilder
{
    Task<bool> ExistsAsync(string exchange, CancellationToken cancellation = default);
    
    Task DeclareAsync(string exchange, string type, Action<IExchangeDeclaration> builder = null, CancellationToken cancellation = default);
    
    Task DeleteAsync(string exchange, Action<IExchangeDeletion> builder = null, CancellationToken cancellation = default);

    Task BindAsync(string destination, string source, string routingKey = null, Action<IBindingDeclaration> builder = null, CancellationToken cancellation = default);

    Task UnbindAsync(string destination, string source, string routingKey = null, Action<IBindingDeclaration> builder = null, CancellationToken cancellation = default);
}