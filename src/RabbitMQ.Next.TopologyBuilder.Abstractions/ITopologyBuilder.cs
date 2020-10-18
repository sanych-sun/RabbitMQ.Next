using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;

namespace RabbitMQ.Next.TopologyBuilder.Abstractions
{
    public interface ITopologyBuilder
    {
        IConnection Connection { get; }

        Task DeclareExchangeAsync(string name, string type, Action<IExchangeBuilder> builder = null);

        Task DeclareQueueAsync(string name, Action<IQueueBuilder> builder = null);
    }
}