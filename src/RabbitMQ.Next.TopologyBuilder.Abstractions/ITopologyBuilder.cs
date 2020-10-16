using System;
using System.Threading.Tasks;

namespace RabbitMQ.Next.TopologyBuilder.Abstractions
{
    public interface ITopologyBuilder
    {
        Task DeclareExchangeAsync(string name, string type, Action<IExchangeBuilder> builder = null);

        Task DeclareQueueAsync(string name, Action<IQueueBuilder> builder = null);

        Task BindAsync(string source, BindingTarget type, string destination, Action<IBindingBuilder> builder = null);
    }
}