using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;

namespace RabbitMQ.Next.TopologyBuilder.Abstractions
{
    public interface ITopologyBuilder : IAsyncDisposable
    {
        IConnection Connection { get; }

        Task DeclareExchangeAsync(string name, string type, Action<IExchangeBuilder> builder = null);

        Task BindExchangeAsync(string destination, string source, Action<IExchangeBindingBuilder> builder = null);

        Task DeclareQueueAsync(string name, Action<IQueueBuilder> builder = null);

        Task BindQueueAsync(string queue, string exchange, Action<IQueueBindingBuilder> builder = null);
    }
}