using System.Collections.Generic;
using RabbitMQ.Next.TopologyBuilder.Abstractions;
using RabbitMQ.Next.Transport.Methods.Exchange;

namespace RabbitMQ.Next.TopologyBuilder
{
    internal class ExchangeBindingBuilder : IExchangeBindingBuilder
    {
        private Dictionary<string, object> arguments;

        public ExchangeBindingBuilder(string source, string destination)
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
    }
}