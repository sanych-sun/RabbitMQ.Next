using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Exceptions;
using RabbitMQ.Next.Transport.Methods.Exchange;

namespace RabbitMQ.Next.TopologyBuilder.Commands
{
    internal class ExchangeBindCommand : IExchangeBindingBuilder, ICommand
    {
        private Dictionary<string, object> arguments;
        private List<string> routingKeys;

        public ExchangeBindCommand(string destination, string source)
        {
            this.Source = source;
            this.Destination = destination;
        }
        
        public string Source { get; }

        public string Destination { get; }

        public IExchangeBindingBuilder RoutingKey(string routingKey)
        {
            this.routingKeys ??= new List<string>();
            this.routingKeys.Add(routingKey);

            return this;
        }

        public IExchangeBindingBuilder Argument(string key, object value)
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
                            new BindMethod(this.Destination, this.Source, this.routingKeys[i], this.arguments));
                    }
                }
                else
                {
                    await channel.SendAsync<BindMethod, BindOkMethod>(
                        new BindMethod(this.Destination, this.Source, null, this.arguments));
                }
            }
            catch (ChannelException ex)
            {
                switch (ex.ErrorCode)
                {
                    case (ushort)ReplyCode.NotFound:
                        throw new ArgumentOutOfRangeException("Source or destination exchange does not exists", ex);
                }
                throw;
            }
        }
    }
}