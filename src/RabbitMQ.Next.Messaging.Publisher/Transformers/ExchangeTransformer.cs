using RabbitMQ.Next.MessagePublisher.Abstractions.Transformers;

namespace RabbitMQ.Next.MessagePublisher.Transformers
{
    public class ExchangeTransformer : IMessageTransformer
    {
        private readonly string exchange;

        public ExchangeTransformer(string exchange)
        {
            this.exchange = exchange;
        }

        public void Apply<TPayload>(TPayload payload, IMessageBuilder message)
        {
            if (string.IsNullOrEmpty(message.Exchange))
            {
                message.Exchange = this.exchange;
            }
        }
    }
}