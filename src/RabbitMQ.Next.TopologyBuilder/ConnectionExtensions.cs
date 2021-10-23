using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.TopologyBuilder.Builders;

namespace RabbitMQ.Next.TopologyBuilder
{
    public static class ConnectionExtensions
    {
        public static Task DeclareExchangeAsync(this IConnection connection, string name, string type, Action<IExchangeBuilder> builder = null)
        {
            var exchangeBuilder = new ExchangeBuilder(name, type);
            builder?.Invoke(exchangeBuilder);

            return connection.UseChannelAsync(exchangeBuilder, (b, c) => b.ApplyAsync(c));
        }

        public static Task BindExchangeAsync(this IConnection connection, string destination, string source, Action<IExchangeBindingBuilder> builder = null)
        {
            var exchangeBindingBuilder = new ExchangeBindingBuilder(destination, source);
            builder?.Invoke(exchangeBindingBuilder);

            return connection.UseChannelAsync(exchangeBindingBuilder, (b, c) => b.ApplyAsync(c));
        }

        public static Task DeclareQueueAsync(this IConnection connection, string name, Action<IQueueBuilder> builder = null)
        {
            var queueBuilder = new QueueBuilder(name);
            builder?.Invoke(queueBuilder);

            return connection.UseChannelAsync(queueBuilder, (b, c) => b.ApplyAsync(c));
        }

        public static Task BindQueueAsync(this IConnection connection, string queue, string exchange, Action<IQueueBindingBuilder> builder = null)
        {
            var queueBindingBuilder = new QueueBindingBuilder(queue, exchange);
            builder?.Invoke(queueBindingBuilder);

            return connection.UseChannelAsync(queueBindingBuilder, (b, c) => b.ApplyAsync(c));
        }
    }
}