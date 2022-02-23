using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Exceptions;
using RabbitMQ.Next.Transport.Methods.Queue;

namespace RabbitMQ.Next.TopologyBuilder.Commands
{
    internal class QueueBindCommand : IQueueBindingBuilder, ICommand
    {
        private Dictionary<string, object> arguments;
        private List<string> routingKeys;

        public QueueBindCommand(string queue, string exchange)
        {
            this.Exchange = exchange;
            this.Queue = queue;
        }
        
        public string Exchange { get; }

        public string Queue { get; }

        public IQueueBindingBuilder RoutingKey(string routingKey)
        {
            this.routingKeys ??= new List<string>();
            this.routingKeys.Add(routingKey);

            return this;
        }

        public IQueueBindingBuilder Argument(string key, object value)
        {
            this.arguments ??= new Dictionary<string, object>();
            this.arguments[key] = value;

            return this;
        }

        public async Task ExecuteAsync(IChannel channel)
        {
            try
            {
                if (this.routingKeys != null && this.routingKeys.Count > 0)
                {
                    for (var i = 0; i < this.routingKeys.Count; i++)
                    {
                        await channel.SendAsync<BindMethod, BindOkMethod>(
                            new BindMethod(this.Queue, this.Exchange, this.routingKeys[i], this.arguments));
                    }
                }
                else
                {
                    await channel.SendAsync<BindMethod, BindOkMethod>(
                        new BindMethod(this.Queue, this.Exchange, null, this.arguments));
                }
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