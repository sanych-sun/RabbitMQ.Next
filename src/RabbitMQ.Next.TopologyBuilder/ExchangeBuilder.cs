using System;
using System.Collections.Generic;
using RabbitMQ.Next.TopologyBuilder.Abstractions;
using RabbitMQ.Next.Transport.Methods.Exchange;

namespace RabbitMQ.Next.TopologyBuilder
{
    internal class ExchangeBuilder : IExchangeBuilder
    {
        private Dictionary<string, object> arguments;
        private List<ExchangeBindingBuilder> bindings;

        public ExchangeBuilder(string name, string type)
        {
            this.Name = name;
            this.Type = type;
        }

        public string Name { get; }

        public string Type { get; }

        public ExchangeFlags Flags { get; set; }

        public void SetArgument(string key, object value)
        {
            this.arguments ??= new Dictionary<string, object>();
            this.arguments[key] = value;
        }

        public IReadOnlyList<ExchangeBindingBuilder> Bindings => this.bindings;

        public void BindTo(string exchange, Action<IExchangeBindingBuilder> builder = null)
        {
            var binding = new ExchangeBindingBuilder(exchange, this.Name);
            builder?.Invoke(binding);

            this.bindings ??= new List<ExchangeBindingBuilder>();
            this.bindings.Add(binding);
        }

        public DeclareMethod ToMethod()
            => new DeclareMethod(this.Name, this.Type, (byte)this.Flags, this.arguments);
    }
}