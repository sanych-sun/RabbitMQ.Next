using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.TopologyBuilder.Abstractions;

namespace RabbitMQ.Next.TopologyBuilder
{
    internal class TopologyBuilder : ITopologyBuilder
    {
        public TopologyBuilder(IConnection connection)
        {
            this.Connection = connection;
        }

        public IConnection Connection { get; }

        public Task DeclareExchangeAsync(string name, string type, Action<IExchangeBuilder> builder = null)
        {
            var exchangeBuilder = new ExchangeBuilder(name, type);
            builder?.Invoke(exchangeBuilder);

            return this.Connection.UseChannelAsync(exchangeBuilder, (b, c) => b.ApplyAsync(c));
        }

        public Task BindExchangeAsync(string destination, string source, Action<IExchangeBindingBuilder> builder = null)
        {
            var exchangeBindingBuilder = new ExchangeBindingBuilder(destination, source);
            builder?.Invoke(exchangeBindingBuilder);

            return this.Connection.UseChannelAsync(exchangeBindingBuilder, (b, c) => b.ApplyAsync(c));
        }

        public Task DeclareQueueAsync(string name, Action<IQueueBuilder> builder = null)
        {
            var queueBuilder = new QueueBuilder(name);
            builder?.Invoke(queueBuilder);

            return this.Connection.UseChannelAsync(queueBuilder, (b, c) => b.ApplyAsync(c));
        }

        public Task BindQueueAsync(string queue, string exchange, Action<IQueueBindingBuilder> builder = null)
        {
            var queueBindingBuilder = new QueueBindingBuilder(queue, exchange);
            builder?.Invoke(queueBindingBuilder);

            return this.Connection.UseChannelAsync(queueBindingBuilder, (b, c) => b.ApplyAsync(c));
        }
    }
}