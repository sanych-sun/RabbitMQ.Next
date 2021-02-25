using RabbitMQ.Next.Publisher.Abstractions.Transformers;

namespace RabbitMQ.Next.Publisher.Transformers
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
                message.SetExchange(this.exchange);
            }
        }
    }
}