using RabbitMQ.Next.MessagePublisher.Abstractions;

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

        public void Apply<TPayload>(TPayload payload, MessageHeader header)
        {
            if (!header.Properties.Headers.ContainsKey(this.key))
            {
                header.Properties.Headers[this.key] = this.value;
            }
        }
    }
}