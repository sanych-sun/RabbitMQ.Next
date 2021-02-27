using System.Collections.Generic;

namespace RabbitMQ.Next.Consumer.Abstractions
{
    public interface IQueueConsumerBuilder
    {
        public bool NoLocal { get; set; }

        public bool Exclusive { get; set; }

        public IDictionary<string, string> Arguments { get; }

        public string ConsumerTag { get; set; }
    }
}