using RabbitMQ.Next.MessagePublisher.Abstractions;

namespace RabbitMQ.Next.MessagePublisher.Transformers
{
    public class ExchangeTransformer : IMessageTransformer
    {
        private readonly string exchange;

        public ExchangeTransformer(string exchange)
        {
            this.exchange = exchange;
        }

        public void Apply<TPayload>(TPayload payload, MessageHeader header)
        {
            if (string.IsNullOrEmpty(header.Exchange))
            {
                header.Exchange = this.exchange;
            }
        }
    }
}