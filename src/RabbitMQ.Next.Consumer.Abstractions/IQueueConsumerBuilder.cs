using System.Collections.Generic;

namespace RabbitMQ.Next.Consumer.Abstractions
{
    public interface IQueueConsumerBuilder
    {
        public string Queue { get; }

        public bool NoLocal { get; set; }

        public bool Exclusive { get; set; }

        public IDictionary<string, object> Arguments { get; }

        public string ConsumerTag { get; set; }
    }
}