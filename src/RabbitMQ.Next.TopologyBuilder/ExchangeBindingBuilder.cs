using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Abstractions.Exceptions;
using RabbitMQ.Next.TopologyBuilder.Abstractions;
using RabbitMQ.Next.Transport.Methods.Exchange;

namespace RabbitMQ.Next.TopologyBuilder
{
    internal class ExchangeBindingBuilder : IExchangeBindingBuilder
    {
        private Dictionary<string, object> arguments;

        public ExchangeBindingBuilder(string destination, string source)
        {
            this.Source = source;
            this.Destination = destination;
        }
        
        public string Source { get; }

        public string Destination { get; }

        public string RoutingKey { get; set; }

        public void SetArgument(string key, object value)
        {
            this.arguments ??= new Dictionary<string, object>();
            this.arguments[key] = value;
        }

        public BindMethod ToMethod()
            => new BindMethod(this.Destination, this.Source, this.RoutingKey, this.arguments);

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
                        throw new ArgumentOutOfRangeException("Source or destination exchange does not exists", ex);
                }
                throw;
            }
        }
    }
}