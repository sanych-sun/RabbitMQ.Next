using System;
using System.Collections.Generic;
using RabbitMQ.Next.TopologyBuilder.Abstractions;
using RabbitMQ.Next.Transport.Methods.Queue;

namespace RabbitMQ.Next.TopologyBuilder
{
    internal class QueueBuilder : IQueueBuilder
    {
        private Dictionary<string, object> arguments;
        private List<QueueBindingBuilder> bindings;

        public QueueBuilder(string name)
        {
            this.Name = name;
        }

        public string Name { get; }

        public QueueFlags Flags { get; set; }

        public void SetArgument(string key, object value)
        {
            this.arguments ??= new Dictionary<string, object>();
            this.arguments[key] = value;
        }

        public IReadOnlyList<QueueBindingBuilder> Bindings => this.bindings;

        public void BindTo(string exchange, Action<IQueueBindingBuilder> builder = null)
        {
            var binding = new QueueBindingBuilder(exchange, this.Name);
            builder?.Invoke(binding);

            this.bindings ??= new List<QueueBindingBuilder>();
            this.bindings.Add(binding);
        }

        public DeclareMethod ToMethod()
            => new DeclareMethod(this.Name, (byte)this.Flags, this.arguments);
    }
}