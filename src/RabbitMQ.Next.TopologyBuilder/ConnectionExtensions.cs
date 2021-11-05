using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.TopologyBuilder.Commands;

namespace RabbitMQ.Next.TopologyBuilder
{
    public static class ConnectionExtensions
    {
        public static Task ExchangeDeclareAsync(this IConnection connection, string name, string type, Action<IExchangeBuilder> builder = null)
            => ExecuteCommand(connection, new ExchangeDeclareCommand(name, type), builder);

        public static Task ExchangeBindAsync(this IConnection connection, string destination, string source, Action<IExchangeBindingBuilder> builder = null)
            => ExecuteCommand(connection, new ExchangeBindCommand(destination, source), builder);

        public static Task ExchangeUnbindAsync(this IConnection connection, string destination, string source, Action<IExchangeBindingBuilder> builder = null)
            => ExecuteCommand(connection, new ExchangeUnbindCommand(destination, source), builder);

        public static Task ExchangeDeleteAsync(this IConnection connection, string destination, Action<IExchangeDeleteBuilder> builder = null)
            => ExecuteCommand(connection, new ExchangeDeleteCommand(destination), builder);

        public static Task QueueDeclareAsync(this IConnection connection, string name, Action<IQueueBuilder> builder = null)
            => ExecuteCommand(connection, new QueueDeclareCommand(name), builder);

        public static Task QueueBindAsync(this IConnection connection, string queue, string exchange, Action<IQueueBindingBuilder> builder = null)
            => ExecuteCommand(connection, new QueueBindCommand(queue, exchange), builder);

        public static Task QueueUnbindAsync(this IConnection connection, string queue, string exchange, Action<IQueueBindingBuilder> builder = null)
            => ExecuteCommand(connection, new QueueUnbindCommand(queue, exchange), builder);

        public static Task QueuePurgeAsync(this IConnection connection, string queue)
            => ExecuteCommand<QueuePurgeCommand, QueuePurgeCommand>(connection, new QueuePurgeCommand(queue), null);

        public static Task QueueDeleteAsync(this IConnection connection, string queue, Action<IQueueDeleteBuilder> builder = null)
            => ExecuteCommand(connection, new QueueDeleteCommand(queue), builder);

        private static Task ExecuteCommand<TBuilder, TCommand>(IConnection connection, TCommand cmd, Action<TBuilder> builder)
            where TCommand: TBuilder, ICommand
        {
            builder?.Invoke(cmd);
            return connection.UseChannelAsync(cmd, (cm, ch) => cm.ExecuteAsync(ch));
        }
    }
}