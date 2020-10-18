using System.Collections.Generic;
using RabbitMQ.Next.TopologyBuilder.Abstractions;
using RabbitMQ.Next.Transport.Methods.Queue;

namespace RabbitMQ.Next.TopologyBuilder
{
    internal class QueueBindingBuilder : IQueueBindingBuilder
    {
        private Dictionary<string, object> arguments;

        public QueueBindingBuilder(string exchange, string queue)
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
    }
}