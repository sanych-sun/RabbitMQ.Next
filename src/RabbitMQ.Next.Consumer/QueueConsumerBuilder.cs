using System.Collections.Generic;
using RabbitMQ.Next.Consumer.Abstractions;

namespace RabbitMQ.Next.Consumer
{
    internal class QueueConsumerBuilder : IQueueConsumerBuilder
    {
        public QueueConsumerBuilder(string queue)
        {
            this.Queue = queue;
            this.Arguments = new Dictionary<string, object>();
        }

        public string Queue { get; }
        public bool NoLocal { get; set; }
        public bool Exclusive { get; set; }
        public Dictionary<string, object> Arguments { get; }
        public string ConsumerTag { get; set; }

        IDictionary<string, object> IQueueConsumerBuilder.Arguments => this.Arguments;
    }
}