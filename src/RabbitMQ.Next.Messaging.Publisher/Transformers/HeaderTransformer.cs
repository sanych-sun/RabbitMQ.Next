using RabbitMQ.Next.MessagePublisher.Abstractions.Transformers;

namespace RabbitMQ.Next.MessagePublisher.Transformers
{
    public class HeaderTransformer : IMessageTransformer
    {
        private readonly string key;
        private readonly string value;

        public HeaderTransformer(string key, string value)
        {
            this.key = key;
            this.value = value;
        }

        public void Apply<TPayload>(TPayload payload, IMessageBuilder message)
        {
            if (message.Headers == null || !message.Headers.ContainsKey(this.key))
            {
                message.SetHeader(this.key, this.value);
            }
        }
    }
}