using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Exceptions;
using RabbitMQ.Next.TopologyBuilder.Abstractions;
using RabbitMQ.Next.Transport.Methods.Queue;

namespace RabbitMQ.Next.TopologyBuilder
{
    internal class QueueBindingBuilder : IQueueBindingBuilder
    {
        private Dictionary<string, object> arguments;

        public QueueBindingBuilder(string queue, string exchange)
        {
            this.Exchange = exchange;
            this.Queue = queue;
        }
        
        public string Exchange { get; }

        public string Queue { get; }

        public string RoutingKey { get; set; }

        public void SetArgument(string key, object value)
        {
            this.arguments ??= new Dictionary<string, object>();
            this.arguments[key] = value;
        }

        public BindMethod ToMethod()
            => new BindMethod(this.Queue, this.Exchange, this.RoutingKey, this.arguments);

        public async Task ApplyAsync(IChannel channel)
        {
            try
            {
                await channel.SendAsync<BindMethod, BindOkMethod>(this.ToMethod());
            }
            catch (ChannelException ex)
            {
                switch (ex.ErrorCode)
                {
                    case (ushort)ReplyCode.NotFound:
                        throw new ArgumentOutOfRangeException("Queue or exchange does not exists", ex);
                }
                throw;
            }
        }
    }
}