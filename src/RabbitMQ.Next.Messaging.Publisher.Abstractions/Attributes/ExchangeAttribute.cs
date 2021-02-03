using System;

namespace RabbitMQ.Next.MessagePublisher.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false)]
    public class ExchangeAttribute : Attribute
    {
        public ExchangeAttribute(string exchange)
        {
            this.Exchange = exchange;
        }

        public string Exchange { get; }
    }
}