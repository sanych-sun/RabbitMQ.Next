using System.Collections.Generic;
using RabbitMQ.Next.Consumer.Abstractions;

namespace RabbitMQ.Next.Consumer
{
    internal class QueueConsumerBuilder : IQueueConsumerBuilder
    {
        public QueueConsumerBuilder(string queue)
        {
            this.Queue = queue;
        }

        public string Queue { get; }

        public bool NoLocal { get; private set; }
        public bool Exclusive { get; private set; }
        public Dictionary<string, object> Arguments { get; private set; }
        public string ConsumerTag { get; set; }

        IQueueConsumerBuilder IQueueConsumerBuilder.NoLocal()
        {
            this.NoLocal = true;
            return this;
        }

        IQueueConsumerBuilder IQueueConsumerBuilder.Exclusive()
        {
            this.Exclusive = true;
            return this;
        }

        public IQueueConsumerBuilder Argument(string key, object value)
        {
            this.Arguments ??= new Dictionary<string, object>();
            this.Arguments[key] = value;
            return this;
        }

        IQueueConsumerBuilder IQueueConsumerBuilder.ConsumerTag(string consumerTag)
        {
            this.ConsumerTag = consumerTag;
            return this;
        }
    }
}