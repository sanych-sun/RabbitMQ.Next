using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.TopologyBuilder.Abstractions;

namespace RabbitMQ.Next.TopologyBuilder
{
    internal class TopologyBuilder : ITopologyBuilder
    {
        private readonly SemaphoreSlim sync = new SemaphoreSlim(1,1);
        private IChannel channel;

        public TopologyBuilder(IConnection connection)
        {
            this.Connection = connection;
        }

        public IConnection Connection { get; }

        public async Task DeclareExchangeAsync(string name, string type, Action<IExchangeBuilder> builder = null)
        {
            var exchangeBuilder = new ExchangeBuilder(name, type);
            builder?.Invoke(exchangeBuilder);

            var ch = await this.GetChannelAsync();
            await exchangeBuilder.ApplyAsync(ch);
        }

        public async Task BindExchangeAsync(string destination, string source, Action<IExchangeBindingBuilder> builder = null)
        {
            var exchangeBindingBuilder = new ExchangeBindingBuilder(destination, source);
            builder?.Invoke(exchangeBindingBuilder);

            var ch = await this.GetChannelAsync();
            await exchangeBindingBuilder.ApplyAsync(ch);
        }

        public async Task DeclareQueueAsync(string name, Action<IQueueBuilder> builder = null)
        {
            var queueBuilder = new QueueBuilder(name);
            builder?.Invoke(queueBuilder);

            var ch = await this.GetChannelAsync();
            await queueBuilder.ApplyAsync(ch);
        }

        public async Task BindQueueAsync(string queue, string exchange, Action<IQueueBindingBuilder> builder = null)
        {
            var queueBindingBuilder = new QueueBindingBuilder(queue, exchange);
            builder?.Invoke(queueBindingBuilder);

            var ch = await this.GetChannelAsync();
            await queueBindingBuilder.ApplyAsync(ch);
        }

        private ValueTask<IChannel> GetChannelAsync()
        {
            var ch = this.channel;
            if (ch == null || ch.Completion.IsCompleted)
            {
                return this.OpenChannelAsync();
            }

            return new ValueTask<IChannel>(ch);
        }

        private async ValueTask<IChannel> OpenChannelAsync()
        {
            if (this.Connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException("Connection should be in Open state to use the TopologyBuilder");
            }

            await this.sync.WaitAsync();

            try
            {
                this.channel ??= await this.Connection.OpenChannelAsync();
                return this.channel;
            }
            finally
            {
                this.sync.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (this.channel != null)
            {
                await this.channel.CloseAsync();
                this.channel = null;
            }
        }
    }
}